using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HttpClient _client;
        public UsuariosController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("MiApi");
        }

        public async Task<IActionResult> Index(string buscar)
        {
            List<Usuario> lista = new List<Usuario>();
            string endpoint = "Usuarios";

            if (!string.IsNullOrEmpty(buscar))
            {
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
                ModelState.AddModelError("", $"Error de conexión: {ex.Message}");
            }

            return View(lista);
        }

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

        public async Task<IActionResult> Edit(int id)
        {
            Usuario usuario = new Usuario();

            try
            {
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

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _client.DeleteAsync($"Usuarios/{id}");
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Index");
        }
    }
}