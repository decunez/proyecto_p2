using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HttpClient _client;

        // CONSTRUCTOR: Inyección de dependencias
        // Aquí pedimos el cliente "MiApi" que configuramos en Program.cs
        public UsuariosController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("MiApi");
        }

        // ---------------------------------------------------
        // 1. LISTAR (INDEX)
        // ---------------------------------------------------
        public async Task<IActionResult> Index(string buscar)
        {
            List<Usuario> lista = new List<Usuario>();

            // Como la base ya es ".../api/", aquí solo ponemos el nombre del controlador
            string endpoint = "Usuarios";

            if (!string.IsNullOrEmpty(buscar))
            {
                // Ruta relativa: api/Usuarios/buscar/{texto}
                endpoint = $"Usuarios/buscar/{buscar}";
            }

            try
            {
                HttpResponseMessage response = await _client.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    lista = JsonConvert.DeserializeObject<List<Usuario>>(data)!;
                }
                else
                {
                    ModelState.AddModelError("", $"Error al cargar lista: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Si la API está apagada, no explota, solo muestra vacío o error
                ModelState.AddModelError("", $"Error de conexión: {ex.Message}");
            }

            return View(lista);
        }

        // ---------------------------------------------------
        // 2. CREAR (CREATE)
        // ---------------------------------------------------
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            try
            {
                string data = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                // Enviamos a: api/Usuarios
                HttpResponseMessage response = await _client.PostAsync("Usuarios", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", $"La API rechazó la creación: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error crítico: {ex.Message}");
            }

            return View(usuario);
        }

        // ---------------------------------------------------
        // 3. EDITAR (EDIT)
        // ---------------------------------------------------
        public async Task<IActionResult> Edit(int id)
        {
            Usuario usuario = new Usuario();

            try
            {
                // Obtenemos: api/Usuarios/{id}
                HttpResponseMessage response = await _client.GetAsync($"Usuarios/{id}");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    usuario = JsonConvert.DeserializeObject<Usuario>(data)!;
                }
                else
                {
                    return Content($"Error al buscar usuario {id}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return Content($"Error de conexión: {ex.Message}");
            }

            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Usuario usuario)
        {
            try
            {
                string data = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                // Enviamos PUT a: api/Usuarios/{id}
                HttpResponseMessage response = await _client.PutAsync($"Usuarios/{usuario.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    string mensajeError = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error API ({response.StatusCode}): {mensajeError}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error de conexión: {ex.Message}");
            }

            return View(usuario);
        }

        // ---------------------------------------------------
        // 4. ELIMINAR (DELETE)
        // ---------------------------------------------------
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Borramos: api/Usuarios/{id}
                await _client.DeleteAsync($"Usuarios/{id}");
            }
            catch (Exception)
            {
                // Ignoramos errores al borrar para no bloquear al usuario
            }

            return RedirectToAction("Index");
        }
    }
}