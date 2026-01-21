using Experimental.System.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para ToListAsync
using Newtonsoft.Json;
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;
using Experimental.System.Messaging;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoporteController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos la base de datos en el constructor
        public SoporteController(AppDbContext context)
        {
            _context = context;
        }

        // 1. MÉTODO PARA RECIBIR TICKET (MODIFICADO PARA GUARDAR EN BD)
        [HttpPost]
        public async Task<IActionResult> RecibirTicket([FromBody] Soporte soporte)
        {
            try
            {
                // A. Validar datos mínimos
                if (soporte == null || string.IsNullOrEmpty(soporte.Descripcion))
                {
                    return BadRequest("Faltan datos del ticket");
                }

                // B. Completar datos automáticos
                soporte.FechaRegistro = DateTime.Now;

                // C. GUARDAR EN LA TABLA SOPORTE (ENCABEZADO)
                _context.Soportes.Add(soporte);
                await _context.SaveChangesAsync(); // Aquí se genera el ID

                // D. (TRUCO) GUARDAR LA DESCRIPCIÓN COMO EL PRIMER MENSAJE DEL CHAT
                // Esto hace que aparezca texto en el chat apenas lo abres
                var primerMensaje = new MensajeChat
                {
                    SoporteId = soporte.Id,   // Usamos el ID que se acaba de crear
                    Contenido = soporte.Descripcion,
                    EsAdmin = false,          // Lo escribió el cliente
                    Fecha = DateTime.Now
                };
                _context.MensajeChat.Add(primerMensaje);
                await _context.SaveChangesAsync();

                return Ok(new { status = "Guardado exitosamente", id = soporte.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al guardar en BD: " + ex.Message);
            }
        }

        // 2. NUEVO MÉTODO PARA LEER (GET)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Soporte>>> GetTickets()
        {
            // Devuelve la lista ordenada por fecha (el más nuevo primero)
            return await _context.Soportes
                                 .OrderByDescending(x => x.FechaRegistro)
                                 .ToListAsync();
        }


        // ... (Tus métodos anteriores siguen igual) ...

        // 3. OBTENER MENSAJES DE UN TICKET ESPECÍFICO
        [HttpGet("{id}/mensajes")]
        public async Task<ActionResult<IEnumerable<MensajeChat>>> GetMensajes(int id)
        {
            return await _context.MensajeChat
                                 .Where(m => m.SoporteId == id)
                                 .OrderBy(m => m.Fecha)
                                 .ToListAsync();
        }

        // 4. RESPONDER (AGREGAR MENSAJE)
        [HttpPost("{id}/mensajes")]
        public async Task<IActionResult> ResponderTicket(int id, [FromBody] MensajeChat mensaje)
        {
            try
            {
                mensaje.SoporteId = id;
                mensaje.Fecha = DateTime.Now;

                _context.MensajeChat.Add(mensaje); // <--- Si esto falla, falta el DbSet en AppDbContext
                await _context.SaveChangesAsync();

                return Ok(new { status = "Mensaje guardado" });
            }
            catch (Exception ex)
            {
                // Esto nos permitirá ver el error real si falla
                return StatusCode(500, ex.Message);
            }
        }
    }
}