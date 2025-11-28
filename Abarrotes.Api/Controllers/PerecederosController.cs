using Abarrotes.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PerecederosController : ControllerBase
{
    private readonly AppDbContext _db;
    public PerecederosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetPorVencer([FromQuery] int dias = 60)
    {
        var hoy = DateTime.UtcNow;

        var lotes = await _db.Lotes
            .Include(l => l.Producto)
                .ThenInclude(p => p.Categoria)
            .Where(l => l.FechaVencimiento != null &&
                        l.CantidadActual > 0)
            .ToListAsync();

        var lista = lotes
            .Select(l => new
            {
                LoteId = l.Id,
                ProductoId = l.ProductoId,
                NombreProducto = l.Producto.Nombre,
                Codigo = l.Producto.CodigoBarras,
                Unidad = l.Producto.EsPorPeso ? "Peso (kg)" : "Unidad",
                Marca = "",
                Presentacion = l.Producto.EsPorPeso ? "KG" : "UND",
                FechaVencimiento = l.FechaVencimiento,
                Cantidad = l.CantidadActual,
                Categoria = l.Producto.Categoria.Nombre,
                Estado = l.CantidadActual <= 0 ? "AGOTADO" : "VIGENTE",

                // ⭐ CALCULO SEGÚN .NET (sin EF)
                DiasRestantes = (l.FechaVencimiento.Value - hoy).Days
            })
            .Where(x => dias == 0 ? x.DiasRestantes <= 0 : x.DiasRestantes <= dias)
            .OrderBy(x => x.FechaVencimiento)
            .ToList();

        return Ok(lista);
    }
}
