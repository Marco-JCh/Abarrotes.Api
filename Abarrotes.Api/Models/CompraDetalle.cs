namespace Abarrotes.Api.Models;

public class CompraDetalle
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public int ProductoId { get; set; }

    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public DateTime? FechaVencimiento { get; set; }

    // Subtotal del ítem = Cantidad * CostoUnitario
    public decimal Subtotal { get; set; }

    public Compra? Compra { get; set; }
    public Producto? Producto { get; set; }
}