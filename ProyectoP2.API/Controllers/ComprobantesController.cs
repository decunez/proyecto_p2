using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Necesario para IEnumerable

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

        // GET: api/Comprobantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comprobante>>> GetComprobantes()
        {
            // CORRECCIÓN 1: Agregamos ThenInclude para que en el Historial salga el nombre del producto
            return await _context.Comprobantes
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto) // <--- ¡ESTO FALTABA!
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        // GET: api/Comprobantes/5 (Para el PDF)
        [HttpGet("{id}")]
        public async Task<ActionResult> GetComprobante(int id)
        {
            var comprobante = await _context.Comprobantes
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)           // <--- ¡AGREGA ESTO!
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comprobante == null)
            {
                return NotFound("No se encontró el comprobante.");
            }

            return Ok(comprobante);
        }

        // POST: api/Comprobantes
        [HttpPost]
        public async Task<ActionResult<Comprobante>> PostComprobante(Comprobante comprobante)
        {
            ModelState.Remove("Usuario");
            ModelState.Remove("Proveedor");

            foreach (var key in ModelState.Keys.Where(k => k.Contains("Producto")).ToList())
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (comprobante.Detalles != null && comprobante.Detalles.Count > 0)
            {
                foreach (var detalle in comprobante.Detalles)
                {
                    // Buscamos el producto original en la BD
                    var productoEnBd = await _context.Productos.FindAsync(detalle.ProductoId);

                    if (productoEnBd != null)
                    {
                        if (comprobante.TipoMovimiento == "ENTRADA")
                        {
                            // Si es compra/alta, SUMAMOS al stock
                            productoEnBd.StockActual += detalle.Cantidad;
                        }
                        else if (comprobante.TipoMovimiento == "SALIDA")
                        {
                            // Si es venta/baja, RESTAMOS al stock
                            productoEnBd.StockActual -= detalle.Cantidad;
                        }

                        // Marcamos el producto como modificado para que se guarde el nuevo stock
                        _context.Entry(productoEnBd).State = EntityState.Modified;
                    }
                }
            }

            // -----------------------------------------------------------------------
            // 3. GUARDAR EN BASE DE DATOS
            // -----------------------------------------------------------------------
            _context.Comprobantes.Add(comprobante);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si hay error interno (ej: ID duplicado), lo mostramos
                return StatusCode(500, "Error interno al guardar: " + ex.Message);
            }

            return CreatedAtAction("GetComprobante", new { id = comprobante.Id }, comprobante);
        }

        // DELETE: api/Comprobantes/5 (Borrar y Revertir Stock)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComprobante(int id)
        {
            var comprobante = await _context.Comprobantes
                                            .Include(c => c.Detalles)
                                            .FirstOrDefaultAsync(c => c.Id == id);

            if (comprobante == null) return NotFound();

            // Revertir Stock
            foreach (var detalle in comprobante.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                {
                    if (comprobante.TipoMovimiento == "ENTRADA")
                    {
                        // Si borro entrada, resto lo que había entrado
                        producto.StockActual -= detalle.Cantidad;
                    }
                    else // SALIDA
                    {
                        // Si borro salida, devuelvo los productos
                        producto.StockActual += detalle.Cantidad;
                    }
                }
            }

            _context.Comprobantes.Remove(comprobante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}