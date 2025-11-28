// Abarrotes.Api/Dtos/VentaDetalleViewDto.cs

using System;
using System.Collections.Generic;

namespace Abarrotes.Api.Dtos
{
    public class VentaDetalleLineaDto
    {
        public string Producto { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class VentaDetalleViewDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoComprobante { get; set; } = null!;
        public string? ComprobanteNumero { get; set; }
        public string Cliente { get; set; } = null!;
        public string MetodoPago { get; set; } = null!;
        public string EstadoPago { get; set; } = null!;
        public decimal Total { get; set; }
        // Totales desglosados
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public List<VentaDetalleLineaDto> Lineas { get; set; } = new();
    }
}
