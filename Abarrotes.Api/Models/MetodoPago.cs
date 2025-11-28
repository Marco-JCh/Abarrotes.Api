using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abarrotes.Api.Models;

[Table("metodopago")]  // tabla real en Postgres (singular y minúscula)
public partial class MetodoPago
{
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    public string? Nombre { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
