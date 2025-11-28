using Abarrotes.Api.Data;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;      // 👈 agrega esto
namespace Abarrotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Vendedor,Inventario")] // 👈 todos pueden leer
public class EstadosProductoController : ControllerBase
{
    private readonly AppDbContext _db;
    public EstadosProductoController(AppDbContext db) => _db = db;

    // GET: /api/EstadosProducto
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstadoProducto>>> GetAll()
    {
        return await _db.EstadosProducto
                        .AsNoTracking()
                        .ToListAsync();
    }
}
