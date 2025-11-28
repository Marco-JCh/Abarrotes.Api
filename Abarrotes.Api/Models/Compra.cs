namespace Abarrotes.Api.Models;

public class Compra
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public DateTime Fecha { get; set; }
    public bool AplicaIgv { get; set; }
    public string? NroComprobante { get; set; }
    public string? Observacion { get; set; }
    // Para “solo registrar” sin afectar inventario (opcional)
    public bool AfectaInventario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }

    public bool Anulada { get; set; } = false;
    public string? TipoPago { get; set; } = "CONTADO";        // CONTADO o CREDITO
    public string? ComprobanteTipo { get; set; } = "FACTURA"; // FACTURA o BOLETA
    public string EstadoCompra { get; set; } = "REGISTRADA"; // REGISTRADA | PENDIENTE | ANULADA

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public Proveedor? Proveedor { get; set; }
    public List<CompraDetalle> Detalles { get; set; } = new();
}