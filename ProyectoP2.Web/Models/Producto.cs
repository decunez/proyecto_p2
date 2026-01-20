using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.Web.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string? CodigoBarra { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }
        public Categoria? Categoria { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}