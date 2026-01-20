using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Data;   // <--- Verifica que coincida con tu carpeta Data
using ProyectoP2.API.Models; // <--- Verifica que coincida con tu carpeta Models
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprobantesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComprobantesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER LISTA DE COMPROBANTES (Para la tabla principal)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comprobante>>> GetComprobantes()
        {
            return await _context.Comprobantes
                .Include(c => c.Proveedor) // <--- ¡ESTO ES VITAL PARA QUE SALGA EL NOMBRE!
                .OrderByDescending(c => c.Fecha) // Ordenar del más reciente al más antiguo
                .ToListAsync();
        }

        // 2. OBTENER UN COMPROBANTE CON DETALLES (Para el Reporte/PDF)
        [HttpGet("{id}")]
        public async Task<ActionResult> GetComprobante(int id)
        {
            var comprobante = await _context.Comprobantes
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)          // <--- Importante: Traer la lista de productos
                    .ThenInclude(d => d.Producto)  // <--- Y el nombre del producto dentro del detalle
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comprobante == null)
            {
                return NotFound("No se encontró el comprobante.");
            }

            return Ok(comprobante);
        }

        // 3. CREAR COMPROBANTE Y ACTUALIZAR STOCK (La parte crítica)
        [HttpPost]
        public async Task<ActionResult> PostComprobante([FromBody] Comprobante comprobante)
        {
            // Validaciones básicas
            if (comprobante == null) return BadRequest("Datos vacíos.");

            // Verificamos si la lista de detalles viene vacía o nula
            if (comprobante.Detalles == null || !comprobante.Detalles.Any())
            {
                return BadRequest("El comprobante no tiene productos. Agregue al menos uno.");
            }

            // --- INICIO DE TRANSACCIÓN ---
            // Usamos transacción para asegurar que si falla el stock, no se guarde el comprobante (y viceversa)
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // PASO A: Recorrer cada producto para actualizar su Stock
                    foreach (var detalle in comprobante.Detalles)
                    {
                        var productoDb = await _context.Productos.FindAsync(detalle.ProductoId);

                        if (productoDb == null)
                        {
                            throw new Exception($"El producto con ID {detalle.ProductoId} no existe.");
                        }

                        // Lógica de ENTRADA o SALIDA
                        if (comprobante.TipoMovimiento == "ENTRADA") // Compra
                        {
                            productoDb.StockActual += detalle.Cantidad;
                        }
                        else if (comprobante.TipoMovimiento == "SALIDA") // Venta
                        {
                            // Validar stock suficiente
                            if (productoDb.StockActual < detalle.Cantidad)
                            {
                                throw new Exception($"Stock insuficiente para '{productoDb.Nombre}'. Tienes {productoDb.StockActual}, intentas sacar {detalle.Cantidad}.");
                            }
                            productoDb.StockActual -= detalle.Cantidad;
                        }

                        // Si quisieras, aquí también podrías actualizar el precio del producto
                        // productoDb.Precio = detalle.PrecioUnitario; 
                    }

                    // PASO B: Guardar el Comprobante
                    // Entity Framework es inteligente: al guardar el Padre (Comprobante),
                    // guarda automáticamente a los Hijos (Detalles) porque están en la lista .Detalles
                    _context.Comprobantes.Add(comprobante);

                    await _context.SaveChangesAsync();

                    // PASO C: Confirmar cambios
                    transaction.Commit();

                    return CreatedAtAction("GetComprobante", new { id = comprobante.Id }, comprobante);
                }
                catch (Exception ex)
                {
                    // Si algo falló, deshacemos todo (Rollback)
                    transaction.Rollback();

                    // Buscamos el mensaje real del error (a veces está en InnerException)
                    var mensajeError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                    return BadRequest($"Error al procesar: {mensajeError}");
                }
            }
        }
    }
}