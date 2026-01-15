using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models // O el namespace que uses en tus modelos
{
    [Table("invent_categorias")] // Mapea a la tabla exacta de la BD
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now; // Valor por defecto
    }
}