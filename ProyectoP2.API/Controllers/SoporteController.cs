using Experimental.System.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json; // Usamos Newtonsoft para compatibilidad total
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoporteController : ControllerBase
    {
        private readonly AppDbContext _context;
        // La ruta de la cola MSMQ
        private const string RUTA_COLA = @".\Private$\cola_soporte_chat";

        public SoporteController(AppDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // 1. CREAR TICKET (Directo a BD para generar ID)
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> RecibirTicket([FromBody] Soporte soporte)
        {
            try
            {
                if (soporte == null || string.IsNullOrEmpty(soporte.Descripcion))
                    return BadRequest("Faltan datos del ticket");

                // 1. Guardar Cabecera
                soporte.FechaRegistro = DateTime.Now;
                _context.Soportes.Add(soporte);
                await _context.SaveChangesAsync();

                // 2. Guardar Primer Mensaje (Descripción)
                var primerMensaje = new MensajeChat
                {
                    SoporteId = soporte.Id,
                    Contenido = soporte.Descripcion,
                    EsAdmin = false, // Lo crea el usuario
                    Fecha = DateTime.Now
                };
                _context.MensajeChat.Add(primerMensaje);
                await _context.SaveChangesAsync();

                return Ok(new { status = "Guardado exitosamente", id = soporte.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error BD: " + ex.Message);
            }
        }

        // ================================================================
        // 2. SINCRONIZAR (Descarga TODOS los mensajes de la cola a la BD)
        // ================================================================
        [HttpGet("sincronizar")]
        public async Task<ActionResult<IEnumerable<MensajeChat>>> SincronizarMensajes()
        {
            var mensajesNuevos = new List<MensajeChat>();

            if (MessageQueue.Exists(RUTA_COLA))
            {
                using (MessageQueue cola = new MessageQueue(RUTA_COLA))
                {
                    // Configuramos el formateador para leer Strings
                    cola.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                    // Bucle infinito que se rompe cuando la cola está vacía
                    while (true)
                    {
                        try
                        {
                            // Intentamos recibir (sacar) un mensaje. Esperamos máx 10ms.
                            // Receive elimina el mensaje de la cola.
                            Message m = cola.Receive(TimeSpan.FromMilliseconds(10));

                            if (m != null && m.Body != null)
                            {
                                string jsonBody = m.Body.ToString();

                                // Convertimos JSON -> Objeto C#
                                var mensajeObj = JsonConvert.DeserializeObject<MensajeChat>(jsonBody);

                                if (mensajeObj != null)
                                {
                                    mensajesNuevos.Add(mensajeObj);
                                }
                            }
                        }
                        catch (MessageQueueException ex)
                        {
                            // Si el error es IOTimeout, significa que la cola ya está vacía
                            if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                            {
                                break; // Salimos del bucle
                            }
                        }
                        catch (Exception)
                        {
                            // Otros errores de lectura se ignoran para no detener el proceso
                        }
                    }
                }
            }

            // Guardamos todo el lote en la Base de Datos
            if (mensajesNuevos.Count > 0)
            {
                _context.MensajeChat.AddRange(mensajesNuevos);
                await _context.SaveChangesAsync();
            }

            // Retornamos la lista completa actualizada desde la BD
            return await _context.MensajeChat
                                 .OrderBy(m => m.Fecha)
                                 .ToListAsync();
        }

        // ================================================================
        // 3. RESPONDER / ENVIAR (Manda a la COLA, no a la BD)
        // ================================================================
        [HttpPost("{id}/mensajes")]
        public IActionResult ResponderTicket(int id, [FromBody] MensajeChat mensaje)
        {
            try
            {
                // Validaciones
                if (mensaje == null || string.IsNullOrEmpty(mensaje.Contenido))
                    return BadRequest("El mensaje no puede estar vacío");

                mensaje.SoporteId = id;
                mensaje.Fecha = DateTime.Now;

                // Verificar existencia de la cola
                if (!MessageQueue.Exists(RUTA_COLA))
                {
                    // Crea una cola estándar (NO Transaccional por defecto)
                    MessageQueue.Create(RUTA_COLA);
                }

                using (MessageQueue cola = new MessageQueue(RUTA_COLA))
                {
                    // Serializamos Objeto -> JSON Texto
                    string jsonMensaje = JsonConvert.SerializeObject(mensaje);

                    // Preparamos el mensaje MSMQ
                    Message msjCola = new Message();
                    msjCola.Label = "ChatSoporte_" + id;
                    msjCola.Body = jsonMensaje;

                    // IMPORTANTE: Definir el formatter antes de enviar asegura compatibilidad
                    msjCola.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                    // ENVÍO INTELIGENTE:
                    // Verifica si la cola es Transaccional para evitar fallos silenciosos
                    if (cola.Transactional)
                    {
                        cola.Send(msjCola, MessageQueueTransactionType.Single);
                    }
                    else
                    {
                        cola.Send(msjCola);
                    }
                }

                return Ok(new { status = "Mensaje en cola de espera" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error Interno: " + ex.Message);
            }
        }

        // ================================================================
        // 4. MÉTODOS DE LECTURA (GETs Standard)
        // ================================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Soporte>>> GetTickets()
        {
            return await _context.Soportes.OrderByDescending(x => x.FechaRegistro).ToListAsync();
        }

        [HttpGet("{id}/mensajes")]
        public async Task<ActionResult<IEnumerable<MensajeChat>>> GetMensajes(int id)
        {
            return await _context.MensajeChat
                                 .Where(m => m.SoporteId == id)
                                 .OrderBy(m => m.Fecha)
                                 .ToListAsync();
        }
    }
}