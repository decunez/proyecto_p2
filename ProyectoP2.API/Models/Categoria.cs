using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models
{
    [Table("invent_categorias")]
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}