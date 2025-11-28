using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abarrotes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        // GET: /api/Dashboard/summary
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            // Stock invertido = Σ (precio * stock real calculado desde lotes)
            var stockInvertido = await _db.Productos
                .Select(p => new
                {
                    p.Id,
                    p.PrecioUnitario,
                    Stock = _db.Lotes
                        .Where(l => l.ProductoId == p.Id)
                        .Sum(l => (decimal?)l.CantidadActual) ?? 0m
                })
                .SumAsync(x => x.PrecioUnitario * x.Stock);

            // Últimos productos (10)
            var ultimos = await _db.Productos
                .OrderByDescending(p => p.Id)
                .Take(10)
                .Select(p => new UltimoProductoDto
                {
                    Codigo = p.CodigoBarras ?? "",
                    Nombre = p.Nombre,
                    Marca = null,
                    Presentacion = p.UnidadBase,   // "UNIDAD", "GRAMO", "KILO"
                    Stock = _db.Lotes
                        .Where(l => l.ProductoId == p.Id)
                        .Sum(l => (decimal?)l.CantidadActual) ?? 0m,
                    Precio = p.PrecioUnitario
                })
                .ToListAsync();

            var dto = new DashboardSummaryDto
            {
                EnCaja = 0,
                ComprasMes = 0,
                VentasDia = 0,
                StockInvertido = stockInvertido,
                UltimosProductos = ultimos
            };

            return Ok(dto);
        }
    }
}
