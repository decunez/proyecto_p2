using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models // <--- Asegúrate que este namespace coincida con tu proyecto
{
    [Table("invent_productos")] // Esto conecta con la tabla SQL que creamos
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        public string? CodigoBarra { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCosto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        // Relaciones
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }

        // Propiedades de Navegación (Virtual para que EF las cargue)
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [ForeignKey("ProveedorId")]
        public virtual Proveedor? Proveedor { get; set; }
    }
}