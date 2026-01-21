using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoP2.Web.Models;
using System.Text;
using System.Text.Json;

namespace ProyectoP2.Web.Controllers
{
    public class ComprobantesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _options;

        public ComprobantesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("MiClienteAPI");
            // Asegúrate de que este puerto coincida con tu API
            _baseUrl = "https://localhost:7232/api/";
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, int? proveedorId, string? tipoMovimiento)
        {
            await CargarListasAuxiliares();

            // Pasamos los datos a la vista para mantener los filtros seleccionados
            ViewBag.Proveedores = ViewBag.ListaProveedores;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            ViewData["TipoMovimiento"] = tipoMovimiento;

            var response = await _httpClient.GetAsync(_baseUrl + "Comprobantes");
            var lista = new List<Comprobante>();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<Comprobante>>(content, _options) ?? new List<Comprobante>();
            }

            // Aplicación de filtros en memoria
            if (fechaInicio.HasValue) lista = lista.Where(x => x.Fecha.Date >= fechaInicio.Value.Date).ToList();
            if (fechaFin.HasValue) lista = lista.Where(x => x.Fecha.Date <= fechaFin.Value.Date).ToList();
            if (proveedorId.HasValue) lista = lista.Where(x => x.ProveedorId == proveedorId.Value).ToList();
            if (!string.IsNullOrEmpty(tipoMovimiento)) lista = lista.Where(x => x.TipoMovimiento == tipoMovimiento).ToList();

            return View(lista.OrderByDescending(x => x.Id).ToList());
        }

        // --- CORRECCIÓN 1: Método GET mejorado ---
        public async Task<IActionResult> Create()
        {
            await CargarListasAuxiliares();

            var comprobante = new Comprobante { Fecha = DateTime.Now };

            // Intentamos identificar al usuario desde que abre el formulario
            var (idUsuario, nombreUsuario) = await ObtenerUsuarioLogueado();

            comprobante.UsuarioId = idUsuario;

            // Enviamos el nombre al ViewBag por si quieres mostrarlo en el formulario (ej: en un input readonly)
            ViewBag.NombreUsuarioDetectado = nombreUsuario;

            return View(comprobante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comprobante comprobante)
        {
            // Ignoramos validaciones de navegación que no vienen del formulario
            ModelState.Remove("Usuario");
            ModelState.Remove("Proveedor");
            ModelState.Remove("Detalles");

            // --- CORRECCIÓN 2: Uso de lógica centralizada y robusta ---
            var (idUsuario, _) = await ObtenerUsuarioLogueado();
            comprobante.UsuarioId = idUsuario;

            comprobante.Estado = "GESTIONADO";
            comprobante.Total = 0; // Se recalculará en la API o Backend

            // Validación defensiva por si el select viene vacío
            if (comprobante.ProveedorId == 0) comprobante.ProveedorId = 1;

            // Limpiamos referencias circulares en detalles
            if (comprobante.Detalles != null)
            {
                foreach (var item in comprobante.Detalles)
                {
                    item.PrecioUnitario = 0;
                    item.Subtotal = 0;
                    item.Producto = null;
                }
            }

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(comprobante);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "Comprobantes", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Error API: " + errorMsg);
                }
            }

            await CargarListasAuxiliares();

            // Recuperamos el nombre para mostrarlo si falla la validación y recarga la vista
            var (_, nombreRecuperado) = await ObtenerUsuarioLogueado();
            ViewBag.NombreUsuarioDetectado = nombreRecuperado;

            return View(comprobante);
        }

        // --- CORRECCIÓN 3: Nuevo método Helper privado para buscar usuario ---
        private async Task<(int Id, string Nombre)> ObtenerUsuarioLogueado()
        {
            // --- PASO 1: RECOLECTAR PISTAS DEL USUARIO ---
            // Buscamos el nombre en Identity, Claims y SESSION (donde probablemente está tu login)
            var posiblesNombres = new List<string>();

            // A. Intentar Identity
            if (!string.IsNullOrEmpty(User.Identity?.Name))
                posiblesNombres.Add(User.Identity.Name);

            // B. Intentar Claims (Cualquier dato adjunto al usuario)
            foreach (var claim in User.Claims)
                posiblesNombres.Add(claim.Value);

            // C. Intentar SESSION (La causa más probable de tu problema)
            try
            {
                // Buscamos claves comunes donde se suele guardar el usuario
                var keys = HttpContext.Session.Keys;
                foreach (var key in keys)
                {
                    var valor = HttpContext.Session.GetString(key);
                    if (!string.IsNullOrEmpty(valor)) posiblesNombres.Add(valor);
                }
            }
            catch { /* Si la sesión no está configurada, ignoramos */ }

            // Limpiamos la lista para buscar: quitamos vacíos, espacios y pasamos a minúsculas
            var pistasLimpias = posiblesNombres
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim().ToLower())
                .ToList();

            // Debug: Para que veas en pantalla qué encontró realmente
            ViewBag.DebugUserLogueado = pistasLimpias.Any()
                ? string.Join(" | ", pistasLimpias)
                : "NADA (El controlador no ve ningún dato de sesión)";

            // --- PASO 2: BUSCAR EN LA BASE DE DATOS ---
            int usuarioIdEncontrado = 1; // Default: Sistema
            string nombreEncontrado = "Sistema";

            try
            {
                var responseUsers = await _httpClient.GetAsync(_baseUrl + "Usuarios");
                if (responseUsers.IsSuccessStatusCode)
                {
                    var contentUsers = await responseUsers.Content.ReadAsStringAsync();
                    var listaUsuarios = JsonSerializer.Deserialize<List<Usuario>>(contentUsers, _options);

                    if (listaUsuarios != null && listaUsuarios.Any())
                    {
                        // Mostramos la lista de la BD para el diagnóstico
                        ViewBag.DebugListaBD = string.Join(" | ", listaUsuarios.Select(u => u.Nombre));

                        Usuario usuarioDetectado = null;

                        // ESTRATEGIA: Ver si alguna pista coincide con algún nombre de la BD
                        foreach (var userBD in listaUsuarios)
                        {
                            if (string.IsNullOrEmpty(userBD.Nombre)) continue;

                            string nombreBD = userBD.Nombre.Trim().ToLower();

                            // Verificamos si alguna de nuestras "pistas" (session/claims) contiene el nombre de la BD
                            // Ejemplo: Pista "daniel@gmail.com" contiene "daniel"
                            // Ejemplo: Pista "Daniel" coincide con "Daniel Cuñez"
                            if (pistasLimpias.Any(pista => pista.Contains(nombreBD) || nombreBD.Contains(pista)))
                            {
                                // Priorizamos la coincidencia más exacta
                                if (usuarioDetectado == null || userBD.Nombre.Length > usuarioDetectado.Nombre.Length)
                                {
                                    usuarioDetectado = userBD;
                                }
                            }
                        }

                        if (usuarioDetectado != null)
                        {
                            usuarioIdEncontrado = usuarioDetectado.Id;
                            nombreEncontrado = usuarioDetectado.Nombre;
                            ViewBag.DebugMatchStatus = $"¡ENCONTRADO! {usuarioDetectado.Nombre} (ID {usuarioDetectado.Id})";
                        }
                        else
                        {
                            ViewBag.DebugMatchStatus = "Sin coincidencias.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.DebugMatchStatus = "Error API: " + ex.Message;
            }

            return (usuarioIdEncontrado, nombreEncontrado);
        }

        private async Task CargarListasAuxiliares()
        {
            var provResponse = await _httpClient.GetAsync(_baseUrl + "Proveedores");
            if (provResponse.IsSuccessStatusCode)
            {
                var content = await provResponse.Content.ReadAsStringAsync();
                var proveedores = JsonSerializer.Deserialize<List<Proveedor>>(content, _options);
                ViewBag.ListaProveedores = new SelectList(proveedores, "Id", "NombreEmpresa");
            }
            else
            {
                ViewBag.ListaProveedores = new SelectList(new List<Proveedor>(), "Id", "NombreEmpresa");
            }

            var prodResponse = await _httpClient.GetAsync(_baseUrl + "Productos");
            if (prodResponse.IsSuccessStatusCode)
            {
                var content = await prodResponse.Content.ReadAsStringAsync();
                var productos = JsonSerializer.Deserialize<List<Producto>>(content, _options);
                ViewBag.ListaProductos = productos;
            }
            else
            {
                ViewBag.ListaProductos = new List<Producto>();
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync(_baseUrl + $"Comprobantes/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudo eliminar.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PdfReporte(int id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"Comprobantes/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var comprobante = JsonSerializer.Deserialize<Comprobante>(content, _options);

                if (comprobante != null && string.IsNullOrEmpty(comprobante.Estado))
                    comprobante.Estado = "GESTIONADO";

                return View(comprobante);
            }
            return Content($"Error al obtener comprobante {id}.");
        }

        public async Task<IActionResult> Historial()
        {
            var response = await _httpClient.GetAsync(_baseUrl + "Comprobantes");
            var listaKardex = new List<KardexItem>();

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var comprobantes = JsonSerializer.Deserialize<List<Comprobante>>(content, _options);

                if (comprobantes != null)
                {
                    foreach (var comp in comprobantes)
                    {
                        if (comp.Detalles != null)
                        {
                            foreach (var detalle in comp.Detalles)
                            {
                                listaKardex.Add(new KardexItem
                                {
                                    Fecha = comp.Fecha,
                                    ComprobanteId = comp.Id,
                                    Tipo = comp.TipoMovimiento,
                                    Producto = detalle.Producto?.Nombre ?? "---",
                                    Cantidad = detalle.Cantidad,
                                    Responsable = comp.Usuario?.Nombre ?? "Sistema"
                                });
                            }
                        }
                    }
                }
            }
            return View(listaKardex.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.ComprobanteId).ToList());
        }
    }

    public class KardexItem
    {
        public DateTime Fecha { get; set; }
        public string Producto { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int Cantidad { get; set; }
        public int ComprobanteId { get; set; }
        public string Responsable { get; set; } = "";
    }
}