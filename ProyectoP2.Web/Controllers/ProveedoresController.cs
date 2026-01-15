using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly HttpClient _client;

        public ProveedoresController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("MiApi");
        }
        // GET: Proveedores (Con Buscador Corregido)
        public async Task<IActionResult> Index(string busqueda)
        {
            List<Proveedor> lista = new List<Proveedor>();
            var response = await _client.GetAsync("Proveedores");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonConvert.DeserializeObject<List<Proveedor>>(json)!;
            }

            // Lógica de búsqueda usando los campos REALES de la BD
            if (!string.IsNullOrEmpty(busqueda))
            {
                busqueda = busqueda.ToLower();
                lista = lista.Where(p =>
                    (p.NombreEmpresa != null && p.NombreEmpresa.ToLower().Contains(busqueda)) ||
                    (p.ContactoNombre != null && p.ContactoNombre.ToLower().Contains(busqueda))
                ).ToList();
            }

            ViewData["BusquedaActual"] = busqueda;
            return View(lista);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(Proveedor proveedor)
        {
            // Asignamos la fecha automáticamente si viene vacía
            if (proveedor.FechaRegistro == default)
                proveedor.FechaRegistro = DateTime.Now;

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(proveedor);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _client.PostAsync("Proveedores", content);
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // 4. EDIT (GET) - Busca el proveedor y rellena el formulario
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"Proveedores/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var proveedor = JsonConvert.DeserializeObject<Proveedor>(json);
                return View(proveedor);
            }

            return NotFound();
        }

        // 5. EDIT (POST) - Guarda los cambios en la API
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(proveedor);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync($"Proveedores/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(proveedor);
        }

        // 6. DELETE (GET) - Muestra confirmación o borra directo (aquí lo hago directo)
        public async Task<IActionResult> Delete(int id)
        {
            // Opcional: Podrías hacer un GET primero para confirmar si existe
            await _client.DeleteAsync($"Proveedores/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}