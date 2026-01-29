using Experimental.System.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoporteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const string RUTA_COLA = @".\Private$\cola_soporte_chat";

        public SoporteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RecibirTicket([FromBody] Soporte soporte)
        {
            try
            {
                if (soporte == null || string.IsNullOrEmpty(soporte.Descripcion))
                    return BadRequest("Faltan datos del ticket");

                soporte.FechaRegistro = DateTime.Now;
                _context.Soportes.Add(soporte);
                await _context.SaveChangesAsync();

                var primerMensaje = new MensajeChat
                {
                    SoporteId = soporte.Id,
                    Contenido = soporte.Descripcion,
                    EsAdmin = false,
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

        [HttpGet("sincronizar")]
        public async Task<ActionResult<IEnumerable<MensajeChat>>> SincronizarMensajes()
        {
            var mensajesNuevos = new List<MensajeChat>();

            if (MessageQueue.Exists(RUTA_COLA))
            {
                using (MessageQueue cola = new MessageQueue(RUTA_COLA))
                {
                    cola.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

                    while (true)
                    {
                        try
                        {

                            Message m = cola.Receive(TimeSpan.FromMilliseconds(10));

                            if (m != null && m.Body != null)
                            {
                                string jsonBody = m.Body.ToString();

                                var mensajeObj = JsonConvert.DeserializeObject<MensajeChat>(jsonBody);

                                if (mensajeObj != null)
                                {
                                    mensajesNuevos.Add(mensajeObj);
                                }
                            }
                        }
                        catch (MessageQueueException ex)
                        {
                            if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                            {
                                break;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            if (mensajesNuevos.Count > 0)
            {
                _context.MensajeChat.AddRange(mensajesNuevos);
                await _context.SaveChangesAsync();
            }

            return await _context.MensajeChat
                                 .OrderBy(m => m.Fecha)
                                 .ToListAsync();
        }

        [HttpPost("{id}/mensajes")]
        public IActionResult ResponderTicket(int id, [FromBody] MensajeChat mensaje)
        {
            try
            {
                if (mensaje == null || string.IsNullOrEmpty(mensaje.Contenido))
                    return BadRequest("El mensaje no puede estar vacío");

                mensaje.SoporteId = id;
                mensaje.Fecha = DateTime.Now;

                if (!MessageQueue.Exists(RUTA_COLA))
                {
                    MessageQueue.Create(RUTA_COLA);
                }

                using (MessageQueue cola = new MessageQueue(RUTA_COLA))
                {
                    string jsonMensaje = JsonConvert.SerializeObject(mensaje);
                    Message msjCola = new Message();
                    msjCola.Label = "ChatSoporte_" + id;
                    msjCola.Body = jsonMensaje;
                    msjCola.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

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