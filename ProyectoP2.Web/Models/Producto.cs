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

        // Relaciones (Para los Dropdowns)
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }

        // Propiedades de navegación (Opcionales, para mostrar nombres en la lista)
        // La API debería devolverte el objeto Categoria dentro del Producto si está configurado así,
        // si no, mostraremos solo el ID por ahora.
        public Categoria? Categoria { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}