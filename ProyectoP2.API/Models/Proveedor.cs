using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models
{
    [Table("invent_proveedores")] // Mapea a la tabla exacta de la BD
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("NombreEmpresa")] // Mapea la columna SQL al código C#
        public string NombreEmpresa { get; set; }

        public string? ContactoNombre { get; set; }

        public string? Telefono { get; set; }

        public string? Correo { get; set; }

        public string? Direccion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}