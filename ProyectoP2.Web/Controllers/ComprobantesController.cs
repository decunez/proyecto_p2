using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoP2.Web.Models;
using System.Text;
using System.Text.Json;

namespace ProyectoP2.Web.Controllers
{
    public class ComprobantesController : Controller
    {
        private readonly HttpClient _httpClient;

        // Opciones para que entienda mayúsculas/minúsculas del JSON
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ComprobantesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MiClienteAPI");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/Comprobantes");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<Comprobante>>(content, _options);
                return View(lista);
            }

            return View(new List<Comprobante>());
        }

        public async Task<IActionResult> Create()
        {
            await CargarListasEnViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comprobante comprobante)
        {
            comprobante.UsuarioId = 1; 
            comprobante.Estado = "A";

            if (comprobante.Detalles != null)
            {
                comprobante.Total = comprobante.Detalles.Sum(x => x.Subtotal);
            }

            var json = JsonSerializer.Serialize(comprobante);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Comprobantes", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Error API: " + errorMsg);

                await CargarListasEnViewBag();
                return View(comprobante);
            }
        }

        public async Task<IActionResult> PdfReporte(int id)
        {
            var response = await _httpClient.GetAsync($"api/Comprobantes/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var comprobante = JsonSerializer.Deserialize<Comprobante>(content, _options);

                return View(comprobante);
            }
            return NotFound();
        }
        private async Task CargarListasEnViewBag()
        {
            var listaProv = new List<Proveedor>();
            var resProv = await _httpClient.GetAsync("api/Proveedores");
            if (resProv.IsSuccessStatusCode)
            {
                var data = await resProv.Content.ReadAsStringAsync();
                listaProv = JsonSerializer.Deserialize<List<Proveedor>>(data, _options);
            }
            ViewBag.ListaProveedores = new SelectList(listaProv, "Id", "NombreEmpresa");

            var listaProd = new List<Producto>();
            var resProd = await _httpClient.GetAsync("api/Productos");
            if (resProd.IsSuccessStatusCode)
            {
                var data = await resProd.Content.ReadAsStringAsync();
                listaProd = JsonSerializer.Deserialize<List<Producto>>(data, _options);
            }
            ViewBag.ListaProductos = listaProd;
        }
    }
}