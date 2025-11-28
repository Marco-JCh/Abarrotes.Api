using System;

namespace Abarrotes.Api.Dtos.Compras
{
    public class CompraItem
    {
        public int ProductoId { get; set; }
        public decimal Cantidad { get; set; }        // CantidadInicial/CantidadActual del lote
        public decimal CostoUnitario { get; set; }   // Costo del lote
        public DateTime? FechaVencimiento { get; set; }  // opcional
        public string? LoteCodigo { get; set; }          // opcional (si algún día lo agregas)
    }
}
