using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models
{
    [Table("invent_comprobantes")] // Nombre exacto de tu tabla
    public class Comprobante
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public string TipoMovimiento { get; set; } // "ENTRADA" o "SALIDA"

        public int? ProveedorId { get; set; }
        [ForeignKey("ProveedorId")]
        public Proveedor? Proveedor { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        // En tu DB es int, así que lo manejamos como int
        public int UsuarioId { get; set; }

        // Mapeo especial: En C# se llama Total, en SQL es TotalComprobante
        [Column("TotalComprobante")]
        public decimal Total { get; set; }

        public string? Estado { get; set; } // Activo/Anulado

        public List<DetalleComprobante> Detalles { get; set; } = new List<DetalleComprobante>();
    }
}