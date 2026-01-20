using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProyectoP2.API.Models
{
    [Table("invent_comprobantes_detalle")] // Nombre exacto de tu tabla hija
    public class DetalleComprobante
    {
        [Key]
        public int Id { get; set; }

        public int ComprobanteId { get; set; }
        [ForeignKey("ComprobanteId")]
        [JsonIgnore] // Importante para evitar errores de ciclo infinito
        public Comprobante? Comprobante { get; set; }

        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}