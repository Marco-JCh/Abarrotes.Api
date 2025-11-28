using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abarrotes.Api.Models
{
    public class HistorialPrecio
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } = null!;

        public decimal PrecioAnterior { get; set; }
        public decimal PrecioNuevo { get; set; }

        public DateTime FechaCambio { get; set; } = DateTime.Now;

        public string Usuario { get; set; } = "Admin"; // opcional
    }
}
