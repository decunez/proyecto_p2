using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
