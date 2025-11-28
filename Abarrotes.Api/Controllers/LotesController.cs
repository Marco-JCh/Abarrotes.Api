using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abarrotes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Inventario,Admin")]
    public class LotesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LotesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoteResponseDto>>> Get([FromQuery] int? productoId = null)
        {
            var q = _db.Lotes.AsNoTracking();
            if (productoId.HasValue) q = q.Where(l => l.ProductoId == productoId.Value);

            var list = await q.OrderBy(l => l.FechaIngreso).ThenBy(l => l.Id)
                .Select(l => new LoteResponseDto(
                    l.Id, l.ProductoId, l.ProveedorId, l.FechaIngreso, l.FechaVencimiento,
                    l.CantidadInicial, l.CantidadActual, l.CostoUnitario))
                .ToListAsync();

            return Ok(list);
        }

        // 🔧 Helper: normaliza DateTime? a UTC
        private static DateTime? AsUtc(DateTime? dt)
        {
            if (dt == null) return null;
            var d = dt.Value;
            return d.Kind switch
            {
                DateTimeKind.Utc => d,
                DateTimeKind.Local => d.ToUniversalTime(),
                _ => DateTime.SpecifyKind(d, DateTimeKind.Utc) // Unspecified -> márcalo como UTC
            };
        }

        [HttpPost]
        public async Task<ActionResult<LoteResponseDto>> Post([FromBody] LoteCreateDto dto)
        {
            if (dto.Cantidad <= 0) return BadRequest("Cantidad debe ser mayor que 0");
            if (dto.CostoUnitario < 0) return BadRequest("CostoUnitario no puede ser negativo");

            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.Id == dto.ProductoId);
            if (producto is null) return NotFound($"Producto {dto.ProductoId} no existe");

            // ✅ FECHAS NORMALIZADAS A UTC (evita el error de Kind=Unspecified)
            var fechaIngreso = AsUtc(dto.FechaIngreso) ?? DateTime.UtcNow;
            var fechaVencimiento = AsUtc(dto.FechaVencimiento);

            var lote = new Lote
            {
                ProductoId = dto.ProductoId,
                ProveedorId = dto.ProveedorId,
                FechaIngreso = fechaIngreso,
                FechaVencimiento = fechaVencimiento,
                CantidadInicial = dto.Cantidad,
                CantidadActual = dto.Cantidad,
                CostoUnitario = dto.CostoUnitario
            };

            _db.Lotes.Add(lote);
            await _db.SaveChangesAsync();

            // Recalcular stock_real del producto a partir de lotes
            producto.StockReal = await _db.Lotes
                .Where(l => l.ProductoId == producto.Id)
                .SumAsync(l => l.CantidadActual);

            await _db.SaveChangesAsync();

            var res = new LoteResponseDto(
                lote.Id, lote.ProductoId, lote.ProveedorId, lote.FechaIngreso, lote.FechaVencimiento,
                lote.CantidadInicial, lote.CantidadActual, lote.CostoUnitario);

            return CreatedAtAction(nameof(Get), new { productoId = res.ProductoId }, res);
        }
        [HttpGet("por-vencer")]
        public async Task<IActionResult> GetPorVencer()
        {
            var hoy = DateTime.UtcNow.Date;        // fecha actual
            var limite = hoy.AddDays(60);          // 60 días

            var lista = await _db.Lotes
                .AsNoTracking()
                .Where(l => l.FechaVencimiento != null &&
                            l.CantidadActual > 0 &&
                            l.FechaVencimiento >= hoy &&
                            l.FechaVencimiento <= limite)
                .Select(l => new
                {
                    l.Id,
                    Producto = l.Producto.Nombre,
                    Unidad = l.Producto.EsPorPeso ? "Peso (kg)" : "Unidad",
                    l.CantidadActual,
                    l.FechaVencimiento,
                    Estado =
                        l.FechaVencimiento < hoy ? "Vencido" :
                        (l.FechaVencimiento <= limite ? "Por vencer" : "Vigente")
                })
                .OrderBy(l => l.FechaVencimiento)
                .ToListAsync();

            return Ok(lista);
        }

    }
}
    