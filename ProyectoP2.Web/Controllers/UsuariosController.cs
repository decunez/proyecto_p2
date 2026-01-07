using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;

namespace ProyectoP2.Web.Controllers
{
    public class UsuariosController : Controller
    {
        // AJUSTA ESTA URL AL PUERTO DE TU API
        private readonly string _baseurl = "https://192.168.100.17.nip.io:7232/api/Usuarios/";
        private readonly HttpClient _client = new HttpClient();

        // LISTAR Y BUSCAR
        public async Task<IActionResult> Index(string buscar)
        {
            List<Usuario> lista = new List<Usuario>();
            string urlFinal = _baseurl;

            if (!string.IsNullOrEmpty(buscar))
            {
                urlFinal = _baseurl + "buscar/" + buscar;
            }

            HttpResponseMessage response = await _client.GetAsync(urlFinal);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                lista = JsonConvert.DeserializeObject<List<Usuario>>(data)!;
            }
            else
            {
                // --- AGREGAMOS ESTO PARA VER EL ERROR ---
                // Si falla, esto nos mostrará el código de error en la pantalla temporalmente
                return Content("ERROR DE CONEXIÓN: " + response.StatusCode.ToString());
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
            string data = JsonConvert.SerializeObject(usuario);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(_baseurl, content);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            return View(usuario);
        }

        // EDITAR - VISTA (GET)
        public async Task<IActionResult> Edit(int id)
        {
            Usuario usuario = new Usuario();
            HttpResponseMessage response = await _client.GetAsync(_baseurl + id);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                usuario = JsonConvert.DeserializeObject<Usuario>(data)!;
            }
            return View(usuario);
        }

        // EDITAR - LOGICA (PUT)
        [HttpPost]
        public async Task<IActionResult> Edit(Usuario usuario)
        {
            string data = JsonConvert.SerializeObject(usuario);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PutAsync(_baseurl + usuario.Id, content);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            return View(usuario);
        }

        // ELIMINAR (DELETE)
        public async Task<IActionResult> Delete(int id)
        {
            HttpResponseMessage response = await _client.DeleteAsync(_baseurl + id);
            return RedirectToAction("Index");
        }
    }
}
