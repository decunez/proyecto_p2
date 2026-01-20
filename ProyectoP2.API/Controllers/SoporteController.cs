using Experimental.System.Messaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.API.Models;
using Experimental.System.Messaging; // Requiere referencia a System.Messaging

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoporteController : ControllerBase
    {
        [HttpPost]
        public IActionResult RecibirTicket([FromBody] Soporte soporte)
        {
            try
            {
                // Ruta de la cola MSMQ
                string rutaCola = @".\Private$\colasoporte";

                if (!MessageQueue.Exists(rutaCola))
                    MessageQueue.Create(rutaCola);

                using (var cola = new MessageQueue(rutaCola))
                {
                    var json = JsonConvert.SerializeObject(soporte);
                    var mensaje = new Message(json);
                    cola.Send(mensaje);
                }

                return Ok(new { status = "Enviado a la cola" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error en API: " + ex.Message);
            }
        }
    }
}