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
        private readonly HttpClient _client;

        // CONSTRUCTOR: Pedimos el cliente configurado en Program.cs
        public AccesoController(IHttpClientFactory httpClientFactory)
        {
            // "MiApi" ya tiene la IP, el puerto y el bypass de seguridad SSL
            _client = httpClientFactory.CreateClient("MiApi");
        }

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

                HttpResponseMessage response = await _client.PostAsync("Usuarios/login", content);

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
                ViewBag.Error = "Error de conexión con el servidor: " + ex.Message;
                return View();
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}