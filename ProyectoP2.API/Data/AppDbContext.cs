using Microsoft.EntityFrameworkCore;
using ProyectoP2.API.Models;

namespace ProyectoP2.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }

        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<ComprobanteDetalle> ComprobanteDetalles { get; set; }

        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Soporte> Soportes { get; set; }
    }
}