using Abarrotes.Api.Data;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProveedoresController(AppDbContext db) => _db = db;

    // GET api/proveedores?search=&page=1&pageSize=10&sort=nombre&dir=asc&incluirInactivos=false
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProveedorDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sort = "nombre",
        [FromQuery] string? dir = "asc",
        [FromQuery] bool incluirInactivos = false) // ✅ nuevo parámetro
    {
        var q = _db.Proveedores.AsNoTracking();

        // ✅ Filtramos los inactivos solo si no se pidió incluirlos
        if (!incluirInactivos)
            q = q.Where(p => p.Activo == true);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(p =>
                p.Nombre.ToLower().Contains(s) ||
                (p.Ruc ?? "").ToLower().Contains(s) ||
                (p.Telefono ?? "").ToLower().Contains(s));
        }

        q = (sort?.ToLower(), dir?.ToLower()) switch
        {
            ("id", "desc") => q.OrderByDescending(p => p.Id),
            ("id", _) => q.OrderBy(p => p.Id),

            ("ruc", "desc") => q.OrderByDescending(p => p.Ruc),
            ("ruc", _) => q.OrderBy(p => p.Ruc),

            ("telefono", "desc") => q.OrderByDescending(p => p.Telefono),
            ("telefono", _) => q.OrderBy(p => p.Telefono),

            ("nombre", "desc") => q.OrderByDescending(p => p.Nombre),
            _ => q.OrderBy(p => p.Nombre)
        };

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProveedorDto(p.Id, p.Nombre, p.Ruc, p.Direccion, p.Telefono, p.Email, p.Activo))
            .ToListAsync();

        return Ok(new PagedResult<ProveedorDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    // GET api/proveedores/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProveedorDto>> GetById(int id)
    {
        var p = await _db.Proveedores.FindAsync(id);
        if (p is null) return NotFound();
        return new ProveedorDto(p.Id, p.Nombre, p.Ruc, p.Direccion, p.Telefono, p.Email, p.Activo);
    }

    // POST api/proveedores
    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> Create([FromBody] ProveedorCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest("El nombre es requerido.");

        var e = new Proveedor
        {
            Nombre = dto.Nombre.Trim(),
            Ruc = dto.Ruc?.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Telefono = dto.Telefono?.Trim(),
            Email = dto.Email?.Trim(),
            Activo = true // ✅ Por defecto siempre activo al crear
        };

        _db.Proveedores.Add(e);
        await _db.SaveChangesAsync();

        var result = new ProveedorDto(e.Id, e.Nombre, e.Ruc, e.Direccion, e.Telefono, e.Email, e.Activo);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, result);
    }

    // PUT api/proveedores/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProveedorUpdateDto dto)
    {
        var e = await _db.Proveedores.FindAsync(id);
        if (e is null) return NotFound();

        e.Nombre = dto.Nombre.Trim();
        e.Ruc = dto.Ruc?.Trim();
        e.Direccion = dto.Direccion?.Trim();
        e.Telefono = dto.Telefono?.Trim();
        e.Email = dto.Email?.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/proveedores/{id}  → inhabilita en lugar de eliminar
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> InhabilitarProveedor(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound();

        proveedor.Activo = false;
        _db.Proveedores.Update(proveedor); // ✅ Asegura que EF detecte el cambio
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ✅ NUEVO: PUT api/proveedores/{id}/reactivar  → reactivar proveedor
    [HttpPut("{id:int}/reactivar")]
    public async Task<IActionResult> ReactivarProveedor(int id)
    {
        var proveedor = await _db.Proveedores.FindAsync(id);
        if (proveedor == null)
            return NotFound();

        proveedor.Activo = true;
        _db.Proveedores.Update(proveedor);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // GET api/proveedores/reporte
    [HttpGet("reporte")]
    public async Task<IActionResult> Reporte([FromQuery] string? search)
    {
        var q = _db.Proveedores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(p => p.Nombre.ToLower().Contains(s) ||
                             (p.Ruc ?? "").ToLower().Contains(s) ||
                             (p.Telefono ?? "").ToLower().Contains(s));
        }

        var data = await q.OrderBy(p => p.Id).ToListAsync();

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Margin(25);
                p.Header().Text("REPORTE DE PROVEEDORES DE ALMACÉN")
                    .FontSize(14).SemiBold().AlignCenter();

                p.Content().Table(t =>
                {
                    t.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(90);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn();
                        cols.RelativeColumn();
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn();
                    });

                    void Head(string s) => t.Cell().Element(Cell).Text(s).SemiBold();

                    t.Header(h =>
                    {
                        Head("Cod. Interno");
                        Head("Proveedor");
                        Head("Teléfono");
                        Head("RUC");
                        Head("Dirección");
                        Head("Email");
                    });

                    foreach (var x in data)
                    {
                        t.Cell().Element(Cell).Text($"PROV{x.Id.ToString().PadLeft(8, '0')}");
                        t.Cell().Element(Cell).Text(x.Nombre);
                        t.Cell().Element(Cell).Text(x.Telefono ?? "-");
                        t.Cell().Element(Cell).Text(x.Ruc ?? "-");
                        t.Cell().Element(Cell).Text(x.Direccion ?? "-");
                        t.Cell().Element(Cell).Text(x.Email ?? "-");
                    }

                    static IContainer Cell(IContainer c) =>
                        c.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });

                p.Footer().AlignRight()
                    .Text($"TOTAL DE PROVEEDORES REGISTRADOS : {data.Count:0}")
                    .FontSize(10);
            });
        });

        var bytes = doc.GeneratePdf();
        return File(bytes, "application/pdf", "Proveedores_Reporte.pdf");
    }
}
