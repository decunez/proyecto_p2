using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <--- NECESARIO

namespace ProyectoP2.API.Models
{
    [Table("Usuario")] // <--- ESTO ES LO QUE ARREGLA EL NOMBRE
    public class Usuario
    {
        [Key] // <--- Indica que 'Id' es la llave primaria
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
    }
}