using System;

namespace Abarrotes.Api.Dtos
{
    public class VentaListadoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        public string TipoComprobante { get; set; } = null!;
        public string ComprobanteNumero { get; set; } = null!;

        public string MetodoPago { get; set; } = null!;
        public decimal Total { get; set; }

        public string Cliente { get; set; } = null!;

        // ✅ NUEVO: nombre del estado de pago (Pagado, Pendiente, etc.)
        public string EstadoPago { get; set; } = null!;
        public decimal MontoPendiente { get; set; }

    }
}
