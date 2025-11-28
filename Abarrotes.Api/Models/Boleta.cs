using System;
using System.Collections.Generic;

namespace Abarrotes.Api.Models;

public partial class Boleta
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public string NumeroBoleta { get; set; } = null!;

    public DateTime FechaEmision { get; set; }

    public virtual Venta Venta { get; set; } = null!;
}
