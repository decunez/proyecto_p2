using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProyectoP2.Web.Models;
using System.Text;
using System.Security.Claims;

namespace ProyectoP2.Web.Controllers
{
    public class AccesoController : Controller
    {
        // NOTA: Si usas el truco de nip.io, cambia la IP aquí abajo por: "https://192.168.100.17.nip.io:7232/api/Usuarios/login"
        private readonly string _baseurl = "https://192.168.100.17:7232/api/Usuarios/login";

        // 1. Aquí solo declaramos la variable, NO la iniciamos todavía
        private readonly HttpClient _client;

        // 2. CONSTRUCTOR (NUEVO): Aquí configuramos el "Bypass" de seguridad SSL
        public AccesoController()
        {
            var handler = new HttpClientHandler();
            // Esta línea mágica permite certificados no seguros (como los de desarrollo)
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            _client = new HttpClient(handler);
        }

        // ---------------------------------------------------------
        //  PARTE 1: LOGIN CON GOOGLE
        // ---------------------------------------------------------

        public IActionResult LoginGoogle()
        {
            var propiedades = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(propiedades, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var resultado = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (resultado.Succeeded)
            {
                var nombre = resultado.Principal.FindFirst(ClaimTypes.Name)?.Value;
                var correo = resultado.Principal.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(nombre))
                {
                    HttpContext.Session.SetString("UsuarioNombre", nombre);
                }
                else
                {
                    HttpContext.Session.SetString("UsuarioNombre", correo ?? "Usuario Google");
                }

                if (correo != null)
                {
                    HttpContext.Session.SetString("UsuarioCorreo", correo);
                }

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login");
        }

        // ---------------------------------------------------------
        //  PARTE 2: LOGIN NORMAL (Base de Datos)
        // ---------------------------------------------------------

        public IActionResult Login()
        {
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
                StringContent content = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                // Ahora _client usará la configuración segura que hicimos en el constructor
                HttpResponseMessage response = await _client.PostAsync(_baseurl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadAsStringAsync();
                    var usuarioEncontrado = JsonConvert.DeserializeObject<Usuario>(resultado);

                    if (usuarioEncontrado != null)
                    {
                        HttpContext.Session.SetString("UsuarioNombre", usuarioEncontrado.Nombre);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
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
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}