using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json; // <--- CAMBIO IMPORTANTE: Usamos la misma que en la API
using ProyectoP2.Web.Models; // Asegúrate de que este namespace sea correcto para tu ViewModel

namespace ProyectoP2.Web.Controllers
{
    public class SoporteController : Controller
    {
        // Verifica que este puerto sea el correcto de tu API
        private readonly string urlApi = "https://localhost:7232/api/Soporte";

        public IActionResult Index()
        {
            return View();
        }

        // =============================================
        // 1. ENVIAR TICKET (Creación inicial)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> EnviarTicket([FromBody] SoporteViewModel data)
        {
            try
            {
                // Bypass SSL para desarrollo
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var datosParaApi = new
                    {
                        NombreCliente = data.NombreCliente,
                        Email = data.Email,
                        Asunto = "Ticket Web",
                        Descripcion = data.Descripcion
                    };

                    // USAMOS NEWTONSOFT (JsonConvert)
                    var jsonContent = new StringContent(
                        JsonConvert.SerializeObject(datosParaApi),
                        Encoding.UTF8,
                        "application/json");

                    var response = await client.PostAsync(urlApi, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        // Leemos la respuesta de la API para obtener el ID generado (opcional)
                        var responseBody = await response.Content.ReadAsStringAsync();
                        return Ok(new { success = true, data = responseBody });
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, $"Error API: {errorMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error Web: {ex.Message}");
            }
        }

        // =============================================
        // 2. BANDEJA (Carga inicial de la tabla)
        // =============================================
        [HttpGet]
        public async Task<IActionResult> Bandeja()
        {
            List<SoporteViewModel> listaTickets = new List<SoporteViewModel>();

            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    var response = await client.GetAsync(urlApi);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();

                        // USAMOS NEWTONSOFT para asegurar compatibilidad con la API
                        listaTickets = JsonConvert.DeserializeObject<List<SoporteViewModel>>(jsonString)
                                       ?? new List<SoporteViewModel>();
                    }
                }
            }
            catch (Exception)
            {
                // Si falla la API, retornamos lista vacía para que no explote la vista
            }

            return View(listaTickets);
        }
    }

    // Tu ViewModel (Asegúrate que esté en el archivo correcto o aquí mismo)
    public class SoporteViewModel
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string Email { get; set; }
        public string Descripcion { get; set; }
        public string Asunto { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}