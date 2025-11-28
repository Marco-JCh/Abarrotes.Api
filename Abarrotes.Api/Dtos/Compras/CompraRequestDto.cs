using System.ComponentModel.DataAnnotations;

namespace Abarrotes.Api.Dtos.Compras;

public class CompraRequestDto
{
    [Required]
    public int ProveedorId { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    public bool AplicaIgv { get; set; } = true;

    public string? NroComprobante { get; set; }
    public string? Observacion { get; set; }

    /// <summary>
    /// Si es true crea lotes y recalcula stock; si false solo registra la compra (histórico).
    /// </summary>
    public bool AfectaInventario { get; set; } = true;

    [MinLength(1, ErrorMessage = "Debe enviar al menos 1 ítem.")]
    public List<CompraItem> Items { get; set; } = new();
}
