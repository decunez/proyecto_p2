using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Necesario para evitar errores de ciclo

namespace ProyectoP2.API.Models // <--- IMPORTANTE: Mismo namespace
{
    [Table("invent_comprobantes_detalles")]
    public class ComprobanteDetalle
    {
        [Key]
        public int Id { get; set; }

        public int ComprobanteId { get; set; } // FK al papá

        public int ProductoId { get; set; } // FK al producto

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]

        public decimal Subtotal { get; set; }

        // --- RELACIONES DE NAVEGACIÓN ---

        [JsonIgnore] // Evita el bucle infinito al convertir a JSON
        [ForeignKey("ComprobanteId")]
        public virtual Comprobante? Comprobante { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto? Producto { get; set; }
    }
}