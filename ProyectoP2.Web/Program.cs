using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// ==================================================================
//  1. CONFIGURACIÓN DE SERVICIOS (DEPENDENCY INJECTION)
// ==================================================================

builder.Services.AddControllersWithViews();

// --- A. CONFIGURACIÓN CENTRALIZADA DEL CLIENTE HTTP (LA SOLUCIÓN) ---
// Leemos la URL que pusiste en appsettings.json
var urlApi = builder.Configuration["ApiUrl"];

// Registramos un cliente llamado "MiApi" que ya sabe cómo conectarse y saltar seguridad SSL
builder.Services.AddHttpClient("MiApi", client =>
{
    // Si olvidaste poner la URL en appsettings, usamos una por defecto para que no explote
    client.BaseAddress = new Uri(urlApi ?? "https://localhost:7232/api/");
    client.Timeout = TimeSpan.FromSeconds(30); // Tiempo máximo de espera
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    // ESTO ES EL TRUCO PARA EL ERROR SSL:
    // Creamos un manejador que acepta certificados de desarrollo (inseguros)
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
    return handler;
});


// --- B. CONFIGURACIÓN OAUTH 2.0 (GOOGLE) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});


// --- C. CONFIGURACIÓN DE SESIÓN ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// ==================================================================
//  2. PIPELINE DE SOLICITUDES (MIDDLEWARE)
// ==================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ¡EL ORDEN ES CRÍTICO AQUÍ!
app.UseSession();        // 1. Activar Sesión
app.UseAuthentication(); // 2. Identificar usuario (Cookie/Google)
app.UseAuthorization();  // 3. Permisos

// Ruta por defecto: Al entrar, te manda al Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();