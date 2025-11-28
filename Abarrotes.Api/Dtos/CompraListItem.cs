namespace Abarrotes.Api.Dtos;

public class CompraListItem
{
    public int Id { get; set; }
    public string ComprobanteTipo { get; set; } = "TICKET"; // ajusta a tu modelo
    public string? NroComprobante { get; set; }
    public DateTime Fecha { get; set; }
    public string Proveedor { get; set; } = "";
    public string TipoPago { get; set; } = "CONTADO";       // ajusta a tu modelo
    public decimal Total { get; set; }        // <-- NECESARIO (corrige CS0117)
    public string Estado { get; set; } = "";          // <-- seguirás enviando aquí
    public string EstadoCompra { get; set; } = "";    // <-- NUEVO: REGISTRADA|PENDIENTE|ANULADA
}
