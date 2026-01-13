using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models // <--- IMPORTANTE: API en mayúsculas
{
    [Table("invent_movimientos")]
    public class MovimientoInventario
    {
        [Key]
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;
        public string? UsuarioResponsable { get; set; }
        public string? Observacion { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }
    }
}