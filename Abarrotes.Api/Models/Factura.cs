using System;
using System.Collections.Generic;

namespace Abarrotes.Api.Models;

public partial class Factura
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Venta Venta { get; set; } = null!;
}
