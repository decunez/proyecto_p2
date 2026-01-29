using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models
{
    [Table("Soporte")]
    public class Soporte
    {
        [Key]
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty; 
        public string? Email { get; set; }
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}