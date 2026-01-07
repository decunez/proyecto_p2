using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios/buscar/juan
        [HttpGet("buscar/{texto}")]
        public async Task<ActionResult<IEnumerable<Usuario>>> BuscarUsuarios(string texto)
        {
            // Busca coincidencias en Nombre O en Correo
            var usuarios = await _context.Usuarios
                .Where(u => u.Nombre.Contains(texto) || u.Correo.Contains(texto))
                .ToListAsync();

            return usuarios;
        }

        // GET: api/Usuarios (BUSCAR TODOS)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5 (BUSCAR POR ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return usuario;
        }

        // POST: api/Usuarios (INSERTAR)
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // PUT: api/Usuarios/5 (MODIFICAR)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id) return BadRequest();
            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Usuarios/5 (ELIMINAR)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Usuarios/login
        [HttpPost("login")]
        public async Task<ActionResult<Usuario>> Login([FromBody] Usuario loginData)
        {
            // Buscamos un usuario que tenga ESE correo Y ESA contraseña
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == loginData.Correo && u.Password == loginData.Password);

            if (usuario == null)
            {
                return NotFound(); // Error 404 si no existe
            }

            return usuario; // Retornamos el usuario si todo está bien
        }
    }
}
