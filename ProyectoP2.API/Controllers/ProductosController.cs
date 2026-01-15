using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Data;    // <--- CAMBIA Api POR API
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {

            return await _context.Productos
                                 .Include(p => p.Categoria)
                                 .Include(p => p.Proveedor)
                                 .ToListAsync();
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return producto;
        }

        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // Iniciamos una transacción (Todo o Nada)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Guardar Producto
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                // 2. Si hay stock inicial, registrar movimiento
                if (producto.StockActual > 0)
                {
                    var movimiento = new MovimientoInventario
                    {
                        ProductoId = producto.Id,
                        TipoMovimiento = "ENTRADA",
                        Cantidad = producto.StockActual,
                        FechaMovimiento = DateTime.Now,
                        UsuarioResponsable = "Admin",
                        Observacion = "Inventario Inicial (Creación)"
                    };
                    _context.MovimientosInventario.Add(movimiento);
                    await _context.SaveChangesAsync();
                }

                // 3. Confirmar que todo salió bien
                await transaction.CommitAsync();

                return CreatedAtAction("GetProducto", new { id = producto.Id }, producto);
            }
            catch (Exception)
            {
                // Si algo falló, deshacemos todo
                await transaction.RollbackAsync();
                throw; // O retornar BadRequest()
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, Producto producto)
        {
            if (id != producto.Id) return BadRequest();

            // Iniciamos la transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Buscamos el producto ORIGINAL en la BD (sin tracking para no bloquear)
                var productoOriginal = await _context.Productos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (productoOriginal == null) return NotFound();

                // 2. Calculamos la diferencia
                int stockAnterior = productoOriginal.StockActual;
                int stockNuevo = producto.StockActual;
                int diferencia = stockNuevo - stockAnterior;

                // 3. Actualizamos el Producto
                _context.Entry(producto).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // 4. Si hubo cambio de stock, registramos el movimiento
                if (diferencia != 0)
                {
                    var movimiento = new MovimientoInventario
                    {
                        ProductoId = producto.Id,
                        FechaMovimiento = DateTime.Now,
                        UsuarioResponsable = "Admin",
                        Cantidad = Math.Abs(diferencia) // Siempre positivo para la cantidad
                    };

                    if (diferencia > 0)
                    {
                        movimiento.TipoMovimiento = "AJUSTE ENTRADA";
                        movimiento.Observacion = $"Ajuste manual: Stock subió de {stockAnterior} a {stockNuevo}";
                    }
                    else
                    {
                        movimiento.TipoMovimiento = "AJUSTE SALIDA";
                        movimiento.Observacion = $"Ajuste manual: Stock bajó de {stockAnterior} a {stockNuevo}";
                    }

                    _context.MovimientosInventario.Add(movimiento);
                    await _context.SaveChangesAsync();
                }

                // 5. Confirmamos la transacción
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!_context.Productos.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}