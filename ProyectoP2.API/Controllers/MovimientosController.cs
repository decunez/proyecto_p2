using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimientosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Movimientos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoInventario>>> GetMovimientos()
        {
            // Incluimos "Producto" para poder mostrar su nombre en la lista
            return await _context.MovimientosInventario
                                 .Include(m => m.Producto)
                                 .OrderByDescending(m => m.FechaMovimiento) // Los más recientes primero
                                 .ToListAsync();
        }
    }
}