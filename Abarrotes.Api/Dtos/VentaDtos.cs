namespace Abarrotes.Api.Dtos;

public class VentaCreate
{
    // "boleta" | "factura"
    public string TipoComprobante { get; set; } = "boleta";

    // opcional: si viene vacío, el backend lo genera
    public string? ComprobanteNumero { get; set; }

    // FKs / opcionales
    public int? MetodoPagoId { get; set; }
    public int? EstadoPagoId { get; set; }
    public int? ClienteId { get; set; }

    public List<DetalleCreate> Detalles { get; set; } = new();

    // Idempotencia: GUID único por click de "Vender"
    public string? RequestKey { get; set; }
}

public class DetalleCreate
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }

    // Si es <= 0, el backend puede tomar el del producto
    public decimal PrecioUnitario { get; set; }
}

public class VentaResponse
{
    public int Id { get; set; }
    public string TipoComprobante { get; set; } = "";
    public string? ComprobanteNumero { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
}
