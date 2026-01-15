using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models; // Asegúrate de tener el modelo aquí
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly HttpClient _client;

        public CategoriasController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("MiApi");
        }

        // 1. INDEX CON BUSCADOR
        public async Task<IActionResult> Index(string busqueda)
        {
            List<Categoria> lista = new List<Categoria>();
            var response = await _client.GetAsync("Categorias");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonConvert.DeserializeObject<List<Categoria>>(json)!;
            }

            if (!string.IsNullOrEmpty(busqueda))
            {
                // Aquí sí usamos "Nombre" porque la tabla Categorias sí tiene esa columna
                lista = lista.Where(c => c.Nombre.ToLower().Contains(busqueda.ToLower())).ToList();
            }

            ViewData["BusquedaActual"] = busqueda;
            return View(lista);
        }

        // 2. CREATE (VISTA)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(categoria);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _client.PostAsync("Categorias", content);
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // 4. EDIT (VISTA)
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"Categorias/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var categoria = JsonConvert.DeserializeObject<Categoria>(json);
                return View(categoria);
            }
            return NotFound();
        }

        // 5. EDIT (ACCIÓN)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(categoria);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _client.PutAsync($"Categorias/{id}", content);
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // 6. DELETE
        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"Categorias/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}