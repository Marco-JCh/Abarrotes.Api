// Abarrotes.Api/Models/Proveedor.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abarrotes.Api.Models;

[Table("proveedores")] // nombre exacto de la tabla en PG
public class Proveedor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }  // SERIAL / IDENTITY en PG

    [Required, StringLength(150)]
    [Column("nombre")]
    public string Nombre { get; set; } = null!;

    [StringLength(20)]
    [Column("ruc")]
    public string? Ruc { get; set; }

    [Column("direccion")]
    public string? Direccion { get; set; }

    [StringLength(20)]
    [Column("telefono")]
    public string? Telefono { get; set; }

    [StringLength(100)]
    [Column("email")]
    public string? Email { get; set; }
    public bool Activo { get; set; } = true; // ✅ nuevo campo

}
