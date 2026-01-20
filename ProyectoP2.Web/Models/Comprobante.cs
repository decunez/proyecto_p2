using System;
using System.Collections.Generic;

namespace ProyectoP2.Web.Models
{
    public class Comprobante
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string TipoMovimiento { get; set; } // ENTRADA o SALIDA

        // Relación con Proveedor
        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Datos de usuario
        public int UsuarioId { get; set; }
        public string? UsuarioResponsable { get; set; } // Opcional, para mostrar nombre

        public decimal Total { get; set; }
        public string? Estado { get; set; }

        // Lista de productos en el comprobante
        public List<DetalleComprobante> Detalles { get; set; } = new List<DetalleComprobante>();
    }
}