using System.Collections.Generic;

namespace Abarrotes.Api.Models
{
    public partial class Venta
    {
        // navegación para la auditoría de consumo por lotes
        public virtual ICollection<ConsumoLote> ConsumosLote { get; set; } = new List<ConsumoLote>();
    }
}
