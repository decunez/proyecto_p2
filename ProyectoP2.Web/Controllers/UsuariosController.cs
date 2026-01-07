using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class UsuariosController : Controller
    {
        // AJUSTA ESTA URL AL PUERTO DE TU API
        // Si usas nip.io, recuerda cambiarlo aquí también, si no, deja la IP.
        private readonly string _baseurl = "https://192.168.100.17:7232/api/Usuarios/";

        // Declaramos la variable pero NO la inicializamos aquí
        private readonly HttpClient _client;

        // --- CONSTRUCTOR NUEVO (LA SOLUCIÓN AL ERROR SSL) ---
        public UsuariosController()
        {
            var handler = new HttpClientHandler();
            // Esto le dice al sistema: "Confía en el certificado aunque sea de desarrollo"
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            _client = new HttpClient(handler);
        }

        // LISTAR Y BUSCAR
        public async Task<IActionResult> Index(string buscar)
        {
            List<Usuario> lista = new List<Usuario>();
            string urlFinal = _baseurl;

            // Si hay texto en buscar, cambiamos la URL
            if (!string.IsNullOrEmpty(buscar))
            {
                // Nota: Asegúrate de que tu API tenga este endpoint "buscar"
                urlFinal = _baseurl + "buscar/" + buscar;
            }

            try
            {
                HttpResponseMessage response = await _client.GetAsync(urlFinal);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    lista = JsonConvert.DeserializeObject<List<Usuario>>(data)!;
                }
                else
                {
                    // Mostramos el error en pantalla si la API responde con error (ej. 404 o 500)
                    return Content($"ERROR API: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                // Mostramos el error si la API está apagada o no se alcanza
                return Content($"ERROR DE CONEXIÓN: {ex.Message}");
            }

            return View(lista);
        }

        // INSERTAR - VISTA
        public IActionResult Create()
        {
            return View();
        }

        // INSERTAR - LOGICA (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            try
            {
                string data = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_baseurl, content);

                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                // Si falla, agregamos el error al modelo para verlo
                ModelState.AddModelError("", "Error al crear en la API: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error de conexión: " + ex.Message);
            }

            return View(usuario);
        }

        // EDITAR - VISTA (GET)
        public async Task<IActionResult> Edit(int id)
        {
            Usuario usuario = new Usuario();

            try
            {
                HttpResponseMessage response = await _client.GetAsync(_baseurl + id);

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    usuario = JsonConvert.DeserializeObject<Usuario>(data)!;
                }
            }
            catch (Exception ex)
            {
                return Content("Error al cargar usuario para editar: " + ex.Message);
            }

            return View(usuario);
        }

        // EDITAR - LOGICA (PUT)
        [HttpPost]
        public async Task<IActionResult> Edit(Usuario usuario)
        {
            try
            {
                string data = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                // Ojo: Asegúrate que tu API espera el ID en la URL para el PUT
                HttpResponseMessage response = await _client.PutAsync(_baseurl + usuario.Id, content);

                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                ModelState.AddModelError("", "Error al editar en la API: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error de conexión: " + ex.Message);
            }

            return View(usuario);
        }

        // ELIMINAR (DELETE)
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                HttpResponseMessage response = await _client.DeleteAsync(_baseurl + id);
            }
            catch (Exception)
            {
                // Podrías manejar el error aquí si quieres
            }

            return RedirectToAction("Index");
        }
    }
}