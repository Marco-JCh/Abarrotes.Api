using System;
using System.Collections.Generic;

namespace Abarrotes.Api.Models;

public partial class Venta
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public int? MetodoPagoId { get; set; }

    public int? EstadoPagoId { get; set; }

    public string? TipoComprobante { get; set; }

    public string? ComprobanteNumero { get; set; }
    public decimal MontoPendiente { get; set; }


    public virtual Boleta? Boleta { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<DetalleVenta> Detalleventa { get; set; } = new List<DetalleVenta>();

    public virtual EstadoPago? EstadoPago { get; set; }

    public virtual Factura? Factura { get; set; }

    public virtual MetodoPago? MetodoPago { get; set; }
}
