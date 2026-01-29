using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;

namespace ProyectoP2.Web.Controllers
{
    public class SoporteController : Controller
    {
        private readonly string urlApi = "https://localhost:7232/api/Soporte";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnviarTicket([FromBody] SoporteViewModel data)
        {
            try
            {
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

                    var jsonContent = new StringContent(
                        JsonConvert.SerializeObject(datosParaApi),
                        Encoding.UTF8,
                        "application/json");

                    var response = await client.PostAsync(urlApi, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
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

                        listaTickets = JsonConvert.DeserializeObject<List<SoporteViewModel>>(jsonString)
                                       ?? new List<SoporteViewModel>();
                    }
                }
            }
            catch (Exception)
            {

            }

            return View(listaTickets);
        }
    }

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