using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos.Categorias;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abarrotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Vendedor,Inventario")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriasController(AppDbContext db) => _db = db;

    // GET: api/Categorias
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll()
    {
        var data = await _db.Categorias
            .AsNoTracking()
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Vigente = c.Vigente
            })
            .ToListAsync();

        return data;
    }

    // POST: api/Categorias
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaDto>> Create(CategoriaCreateDto dto)
    {
        // Validar duplicados
        if (await _db.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower()))
            return BadRequest("La categoría ya existe.");

        var cat = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Vigente = true
        };

        _db.Categorias.Add(cat);
        await _db.SaveChangesAsync();

        return Ok(cat);
    }

    // PUT: api/Categorias/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Edit(int id, CategoriaEditDto dto)
    {
        var cat = await _db.Categorias.FindAsync(id);
        if (cat == null) return NotFound();

        cat.Nombre = dto.Nombre;
        cat.Descripcion = dto.Descripcion;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH: api/Categorias/{id}/estado
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CambiarEstado(int id)
    {
        var cat = await _db.Categorias.FindAsync(id);
        if (cat == null) return NotFound();

        cat.Vigente = !cat.Vigente;
        await _db.SaveChangesAsync();

        return Ok(new { cat.Id, cat.Vigente });
    }
}
