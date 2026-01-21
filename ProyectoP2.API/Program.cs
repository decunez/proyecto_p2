using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERVICIOS
// ==========================================

// Agregar conexión a Base de Datos (SQL Server)
// Asegúrate de que en appsettings.json tu cadena se llame "DefaultConnection"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ?? AQUÍ ESTÁ LA SOLUCIÓN AL PROBLEMA DE CARGA (CORS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()    // Permite que la Web entre a la API
              .AllowAnyMethod()    // Permite GET, POST, PUT, DELETE
              .AllowAnyHeader();   // Permite cualquier tipo de datos
    });
});

builder.Services.AddControllers();
// Configuración para Swagger (Documentación de la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 2. CONFIGURACIÓN DEL PIPELINE (Middleware)
// ==========================================

// Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ?? IMPORTANTE: Activar CORS antes de la autorización
app.UseCors("PermitirTodo");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();