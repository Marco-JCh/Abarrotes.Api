namespace Abarrotes.Api.Dtos
{
    public class DashboardSummaryDto
    {
        public decimal EnCaja { get; set; }          // placeholder por ahora
        public decimal ComprasMes { get; set; }      // placeholder por ahora
        public decimal VentasDia { get; set; }       // placeholder por ahora
        public decimal StockInvertido { get; set; }  // Σ (Producto.PrecioUnitario * Producto.StockReal)
        public List<UltimoProductoDto> UltimosProductos { get; set; } = new();
    }

    public class UltimoProductoDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Marca { get; set; }           // opcional (si luego la tienes)
        public string Presentacion { get; set; } = "";
        public decimal Stock { get; set; }
        public decimal Precio { get; set; }
    }
}
