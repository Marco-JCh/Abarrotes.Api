namespace Abarrotes.Api.Dtos.Compras;

public class CompraResponseDto
{
    public int Id { get; set; }
    public bool AfectaInventario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public int Items { get; set; }
}
