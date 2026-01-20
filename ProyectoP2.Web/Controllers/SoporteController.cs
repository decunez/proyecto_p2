using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ProyectoP2.Web.Controllers
{
    public class SoporteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnviarTicket([FromBody] SoporteViewModel data)
        {
            string urlApi = "https://localhost:7232/api/Soporte";

            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {

                    var datosParaApi = new
                    {
                        NombreCliente = data.NombreCliente,
                        Email = data.Email,    
                        Asunto = data.Asunto ?? "Ticket Web", 
                        Descripcion = data.Descripcion
                    };

                    // 4. CONVERTIR A JSON
                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(datosParaApi),
                        Encoding.UTF8,
                        "application/json");

                    // 5. ENVIAR A LA API
                    var response = await client.PostAsync(urlApi, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { success = true, message = "Ticket creado exitosamente" });
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, $"La API respondió error: {errorMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error de conexión Web-API: {ex.Message}");
            }
        }
    }
    public class SoporteViewModel
    {
        public string NombreCliente { get; set; }
        public string Email { get; set; }  
        public string Descripcion { get; set; }
        public string Asunto { get; set; }
    }
}