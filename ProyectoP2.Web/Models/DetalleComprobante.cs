namespace ProyectoP2.Web.Models
{
    public class DetalleComprobante
    {
        public int Id { get; set; }
        public int ComprobanteId { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; } // Para mostrar el nombre en la tabla

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}