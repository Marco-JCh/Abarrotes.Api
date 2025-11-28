using System;
using System.Collections.Generic;

namespace Abarrotes.Api.Models;

public partial class EstadoPago
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
