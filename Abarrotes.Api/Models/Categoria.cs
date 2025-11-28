using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abarrotes.Api.Models;

public partial class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("vigente")]
    public bool Vigente { get; set; } = true;


    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
