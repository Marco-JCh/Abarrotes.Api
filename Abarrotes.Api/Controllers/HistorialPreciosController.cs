using Abarrotes.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class HistorialPreciosController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public HistorialPreciosController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet("{productoId}")]
    public async Task<IActionResult> GetHistorialPorProducto(int productoId)
    {
        var historial = await _ctx.HistorialPrecios
            .Where(h => h.ProductoId == productoId)
            .OrderByDescending(h => h.FechaCambio)
            .Select(h => new
            {
                h.Id,
                Producto = h.Producto.Nombre,
                h.PrecioAnterior,
                h.PrecioNuevo,
                h.FechaCambio,
                h.Usuario
            })
            .ToListAsync();

        return Ok(historial);
    }

    [HttpGet]
    public async Task<IActionResult> GetHistorialCompleto()
    {
        var historial = await _ctx.HistorialPrecios
            .Include(h => h.Producto)
            .OrderByDescending(h => h.FechaCambio)
            .Select(h => new
            {
                h.Id,
                Producto = h.Producto.Nombre,
                h.PrecioAnterior,
                h.PrecioNuevo,
                h.FechaCambio,
                h.Usuario
            })
            .ToListAsync();

        return Ok(historial);
    }
}
