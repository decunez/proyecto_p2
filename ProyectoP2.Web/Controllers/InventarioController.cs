using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class InventarioController : Controller
    {
        private readonly HttpClient _client;

        public InventarioController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("MiApi");
        }

        // 1. LISTAR PRODUCTOS (CON BÚSQUEDA)
        public async Task<IActionResult> Index(string busqueda) // <--- Agregamos este parámetro
        {
            List<Producto> lista = new List<Producto>();
            try
            {
                // 1. Traemos TODOS los productos de la API
                HttpResponseMessage response = await _client.GetAsync("Productos");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    lista = JsonConvert.DeserializeObject<List<Producto>>(data)!;
                }

                // 2. Si el usuario escribió algo, filtramos la lista aquí mismo
                if (!string.IsNullOrEmpty(busqueda))
                {
                    busqueda = busqueda.ToLower(); // Convertimos a minúsculas para comparar mejor

                    lista = lista.Where(p =>
                        (p.Nombre != null && p.Nombre.ToLower().Contains(busqueda)) ||
                        (p.CodigoBarra != null && p.CodigoBarra.ToLower().Contains(busqueda))
                    ).ToList();
                }

                // Guardamos la búsqueda para que no se borre del cuadro de texto
                ViewData["BusquedaActual"] = busqueda;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error de conexión: " + ex.Message);
            }
            return View(lista);
        }
        public async Task<IActionResult> Create()
        {
            await CargarListas();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Producto producto)
        {
            try
            {
                string data = JsonConvert.SerializeObject(producto);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync("Productos", content);

                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                ModelState.AddModelError("", "Error al guardar en API: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }

            await CargarListas();
            return View(producto);
        }

        // GET: Inventario/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // 1. Buscar el producto
            var response = await _client.GetAsync($"Productos/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var producto = JsonConvert.DeserializeObject<Producto>(json);

            // 2. Cargar Categorías para la lista
            var respCat = await _client.GetAsync("Categorias");
            var categorias = JsonConvert.DeserializeObject<List<Categoria>>(await respCat.Content.ReadAsStringAsync());

            // 3. Cargar Proveedores para la lista
            var respProv = await _client.GetAsync("Proveedores");
            var proveedores = JsonConvert.DeserializeObject<List<Proveedor>>(await respProv.Content.ReadAsStringAsync());

            // 4. Pasar las listas a la Vista (ViewBag)
            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre", producto.CategoriaId);
            // Asegúrate que tu modelo Proveedor tenga "NombreEmpresa" o cambia "Nombre" por la propiedad correcta
            ViewBag.Proveedores = new SelectList(proveedores, "Id", "NombreEmpresa", producto.ProveedorId);

            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Producto producto)
        {
            try
            {
                string data = JsonConvert.SerializeObject(producto);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PutAsync($"Productos/{producto.Id}", content);

                if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            }
            catch (Exception) { }

            await CargarListas();
            return View(producto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"Productos/{id}");
            return RedirectToAction("Index");
        }
        private async Task CargarListas()
        {
            var categorias = new List<Categoria>();
            var proveedores = new List<Proveedor>();

            try
            {
                var resCat = await _client.GetAsync("Categorias");
                if (resCat.IsSuccessStatusCode)
                    categorias = JsonConvert.DeserializeObject<List<Categoria>>(await resCat.Content.ReadAsStringAsync())!;

                var resProv = await _client.GetAsync("Proveedores");
                if (resProv.IsSuccessStatusCode)
                    proveedores = JsonConvert.DeserializeObject<List<Proveedor>>(await resProv.Content.ReadAsStringAsync())!;
            }
            catch { }
            ViewBag.ListaCategorias = new SelectList(categorias, "Id", "Nombre");
            ViewBag.ListaProveedores = new SelectList(proveedores, "Id", "NombreEmpresa");
        }
    }
}