using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoP2.API.Models
{
    [Table("MensajeChat")]
    public class MensajeChat
    {
        [Key]
        public int Id { get; set; }
        public int SoporteId { get; set; } // FK
        public string Contenido { get; set; }
        public bool EsAdmin { get; set; } // true: Admin, false: Cliente
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}