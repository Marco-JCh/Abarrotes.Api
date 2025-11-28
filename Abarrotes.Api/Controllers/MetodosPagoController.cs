// Controllers/MetodosPagoController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Abarrotes.Api.Data;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Authorization;      // 👈 agrega esto


[ApiController]
[Route("api/[controller]")] // → /api/MetodosPago
[Authorize(Roles = "Admin,Vendedor,Inventario")] // 👈 todos pueden leer
public class MetodosPagoController : ControllerBase
{
    private readonly AppDbContext _db;
    public MetodosPagoController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MetodoPago>>> GetAll()
    {
        var list = await _db.MetodosPago.AsNoTracking()
                                        .OrderBy(x => x.Id)
                                        .ToListAsync();
        return Ok(list);
    }
}
