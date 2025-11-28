using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Abarrotes.Api.Models
{
    /// <summary>
    /// Auditoría de consumo por lote en cada venta.
    /// Permite saber exactamente qué lotes aportaron cuánta cantidad.
    /// </summary>
    public class ConsumoLote
    {
        public int Id { get; set; }

        // Clave foránea a la venta
        public int VentaId { get; set; }
        public Venta Venta { get; set; } = null!;

        // Clave foránea al lote consumido
        public int LoteId { get; set; }
        public Lote Lote { get; set; } = null!;

        // Cantidad consumida de ese lote
        [Precision(12, 3)]
        public decimal Cantidad { get; set; }
    }
}
