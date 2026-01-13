using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models
{
    [Table("invent_proveedores")]
    public class Proveedor
    {
        public int Id { get; set; }
        public string NombreEmpresa { get; set; }
        public string? Telefono { get; set; }
    }
}