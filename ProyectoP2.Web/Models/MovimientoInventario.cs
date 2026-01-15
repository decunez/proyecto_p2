namespace ProyectoP2.Web.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; } // 'ENTRADA', 'SALIDA', 'AJUSTE'
        public int Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? UsuarioResponsable { get; set; }
        public string? Observacion { get; set; }

        // Para mostrar el nombre del producto
        public Producto? Producto { get; set; }
    }
}