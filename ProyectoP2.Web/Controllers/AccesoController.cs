using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;
using System.Security.Claims; // Agregado para leer los datos de Google

namespace ProyectoP2.Web.Controllers
{
    public class AccesoController : Controller
    {
        private readonly string _baseurl = "https://192.168.100.17.nip.io:7232/api/Usuarios/login";
        private readonly HttpClient _client = new HttpClient();

        // 1. Iniciar el viaje a Google
        public IActionResult LoginGoogle()
        {
            var propiedades = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(propiedades, GoogleDefaults.AuthenticationScheme);
        }

        // 2. Respuesta de Google (AQUÍ ESTÁ EL CAMBIO IMPORTANTE)
        public async Task<IActionResult> GoogleResponse()
        {
            // Verificamos qué nos respondió Google
            var resultado = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (resultado.Succeeded)
            {
                // Extraemos la información del usuario que viene de Google
                var nombre = resultado.Principal.FindFirst(ClaimTypes.Name)?.Value;
                var correo = resultado.Principal.FindFirst(ClaimTypes.Email)?.Value;

                // Esto es lo que faltaba para que se active tu menú
                if (!string.IsNullOrEmpty(nombre))
                {
                    HttpContext.Session.SetString("UsuarioNombre", nombre);
                }
                else
                {
                    HttpContext.Session.SetString("UsuarioNombre", correo ?? "Usuario Google");
                }

                // Opcional: También puedes guardar el correo si lo necesitas luego
                if (correo != null)
                {
                    HttpContext.Session.SetString("UsuarioCorreo", correo);
                }

                // Redirigimos al Home ya con la sesión iniciada
                return RedirectToAction("Index", "Home");
            }

            // Si falló, lo devolvemos al Login
            return RedirectToAction("Login");
        }


        // ---------------------------------------------------------
        //  PARTE 2: LOGIN NORMAL (Base de Datos)
        // ---------------------------------------------------------

        public IActionResult Login()
        {
            // Si ya hay sesión, no mostrar login, ir directo al Home
            if (HttpContext.Session.GetString("UsuarioNombre") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Usuario usuario)
        {
            try
            {
                // Convertimos el usuario a JSON
                StringContent content = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                // Enviamos los datos a la API
                HttpResponseMessage response = await _client.PostAsync(_baseurl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadAsStringAsync();
                    var usuarioEncontrado = JsonConvert.DeserializeObject<Usuario>(resultado);

                    // Guardamos en sesión
                    if (usuarioEncontrado != null)
                    {
                        HttpContext.Session.SetString("UsuarioNombre", usuarioEncontrado.Nombre);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                // Por si la API está apagada
                ViewBag.Error = "Error de conexión con la API: " + ex.Message;
                return View();
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        // ---------------------------------------------------------
        //  PARTE 3: SALIR
        // ---------------------------------------------------------

        public async Task<IActionResult> Logout()
        {
            // 1. Borramos la sesión de nuestra App
            HttpContext.Session.Clear();

            // 2. Cerramos la sesión de la cookie de Google también (para limpieza total)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}