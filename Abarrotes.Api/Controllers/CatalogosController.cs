using Abarrotes.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abarrotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Vendedor,Inventario")] // 👈 todos pueden leer
public class CatalogosController : ControllerBase
{
    private readonly AppDbContext _db;
    public CatalogosController(AppDbContext db) => _db = db;

    // GET: api/Catalogos/categorias
    [HttpGet("categorias")]
    public async Task<ActionResult<IEnumerable<object>>> GetCategorias()
    {
        var data = await _db.Categorias
            .Select(c => new { c.Id, c.Nombre })
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(data);
    }

    // GET: api/Catalogos/estados
    [HttpGet("estados")]
    public async Task<ActionResult<IEnumerable<object>>> GetEstados()
    {
        var data = await _db.EstadosProducto
            .Select(e => new { e.Id, e.Nombre })
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        return Ok(data);
    }
}
