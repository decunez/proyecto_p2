using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ProyectoP2.Web.Controllers
{
    public class SoporteController : Controller
    {
        // ⚠️ IMPORTANTE: Verifica que este puerto (7232) sea el mismo que sale en la ventana negra de tu API
        private readonly string urlApi = "https://localhost:7232/api/Soporte";

        // 1. VISTA PRINCIPAL (Donde está el botón y el modal)
        public IActionResult Index()
        {
            return View();
        }

        // 2. ENVIAR TICKET (Lo llama el JavaScript del Modal)
        [HttpPost]
        public async Task<IActionResult> EnviarTicket([FromBody] SoporteViewModel data)
        {
            try
            {
                // Configuración para evitar errores de certificado SSL en desarrollo
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient(handler))
                {
                    // Preparamos el objeto para enviar a la API
                    var datosParaApi = new
                    {
                        NombreCliente = data.NombreCliente,
                        Email = data.Email,             // Enviamos el correo real
                        Asunto = "Ticket Web",          // Asunto por defecto
                        Descripcion = data.Descripcion
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(datosParaApi),
                        Encoding.UTF8,
                        "application/json");

                    // Enviamos la petición POST a la API
                    var response = await client.PostAsync(urlApi, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { success = true });
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

        // 3. BANDEJA DE ENTRADA (Muestra la tabla de tickets)
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
                    // Hacemos una petición GET a la API para obtener la lista
                    var response = await client.GetAsync(urlApi);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();

                        // Convertimos el JSON que llega de la API a una lista de C#
                        listaTickets = JsonSerializer.Deserialize<List<SoporteViewModel>>(jsonString,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                    }
                }
            }
            catch (Exception)
            {
                // Si la API está apagada, mostramos la lista vacía para que no se caiga la web
            }

            // Retornamos la Vista 'Bandeja.cshtml' con los datos
            return View(listaTickets);
        }
    }

    // ==========================================
    // MODELO (DTO)
    // ==========================================
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