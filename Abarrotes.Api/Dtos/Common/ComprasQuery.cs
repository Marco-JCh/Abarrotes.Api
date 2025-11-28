namespace Abarrotes.Api.Dtos.Compras;

public class ComprasQuery
{
    public string? Desde { get; set; }     // "YYYY-MM-DD"
    public string? Hasta { get; set; }     // "YYYY-MM-DD"
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Estado { get; set; }    // "VIGENTE" | "ANULADA"
    public string? Pago { get; set; }      // "CONTADO" | "CREDITO" (opcional)
    public string? Sort { get; set; }      // "fecha" | "-fecha"
}
