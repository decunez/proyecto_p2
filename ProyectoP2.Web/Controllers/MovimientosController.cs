using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;

namespace ProyectoP2.Web.Controllers
{
    public class MovimientosController : Controller
    {
        private readonly HttpClient _client;

        public MovimientosController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("MiApi");
        }

        // GET: Movimientos
        public async Task<IActionResult> Index(string busqueda)
        {
            List<MovimientoInventario> lista = new List<MovimientoInventario>();
            try
            {
                // 1. Traemos TODOS los movimientos de la API
                var response = await _client.GetAsync("Movimientos");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    lista = JsonConvert.DeserializeObject<List<MovimientoInventario>>(data)!;
                }

                // 2. FILTRO DE BÚSQUEDA
                if (!string.IsNullOrEmpty(busqueda))
                {
                    busqueda = busqueda.ToLower(); // Convertimos a minúsculas para comparar fácil

                    lista = lista.Where(m =>
                        // Buscar por Nombre de Producto (validando que no sea nulo)
                        (m.Producto != null && m.Producto.Nombre.ToLower().Contains(busqueda)) ||

                        // Buscar por Tipo (Entrada/Salida)
                        (m.TipoMovimiento != null && m.TipoMovimiento.ToLower().Contains(busqueda)) ||

                        // Buscar por Usuario
                        (m.UsuarioResponsable != null && m.UsuarioResponsable.ToLower().Contains(busqueda)) ||

                        // Buscar por Comentario/Observación
                        (m.Observacion != null && m.Observacion.ToLower().Contains(busqueda))
                    ).ToList();
                }

                // Guardamos lo que escribió el usuario para mantenerlo en la cajita
                ViewData["BusquedaActual"] = busqueda;
            }
            catch (Exception)
            {
                // Manejo de errores
            }

            return View(lista);
        }
    }
}