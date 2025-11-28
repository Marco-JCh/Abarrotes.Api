using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Inventario,Admin")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductosController(AppDbContext db) => _db = db;

    // ===========================================================
    // LISTAR TODOS
    // ===========================================================
    [HttpGet]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoReadDto>>> GetAll()
    {
        var items = await _db.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.EstadoProducto)
            .Include(p => p.Lotes)
            .Select(p => new ProductoReadDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                CodigoBarras = p.CodigoBarras,
                PrecioUnitario = p.PrecioUnitario,

                EsPorPeso = p.EsPorPeso,
                UnidadBase = p.UnidadBase,
                FactorBase = p.FactorBase,

                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null,

                EstadoProductoId = p.EstadoProductoId,
                EstadoProductoNombre = p.EstadoProducto != null ? p.EstadoProducto.Nombre : null,

                // ✔ STOCK REAL (sumando lotes)
                StockReal = p.Lotes.Sum(l => l.CantidadActual),

                // ✔ OBJETO COMPLETO
                Categoria = p.Categoria != null
                    ? new CategoriaMini { Id = p.Categoria.Id, Nombre = p.Categoria.Nombre }
                    : null,

                EstadoProducto = p.EstadoProducto != null
                    ? new EstadoProductoMini { Id = p.EstadoProducto.Id, Nombre = p.EstadoProducto.Nombre }
                    : null
            })
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return Ok(items);
    }


    // ===========================================================
    // CREAR
    // ===========================================================
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProductoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Datos inválidos", errors = ModelState });

        if (!string.IsNullOrWhiteSpace(dto.CodigoBarras))
        {
            var exists = await _db.Productos.AnyAsync(p => p.CodigoBarras == dto.CodigoBarras);
            if (exists) return BadRequest(new { message = "El código de barras ya existe." });
        }

        if (dto.CategoriaId.HasValue && !await _db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            return BadRequest(new { message = "La categoría no existe." });

        if (dto.EstadoProductoId.HasValue && !await _db.EstadosProducto.AnyAsync(e => e.Id == dto.EstadoProductoId))
            return BadRequest(new { message = "El estado de producto no existe." });

        var p = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            CodigoBarras = dto.CodigoBarras,
            PrecioUnitario = dto.PrecioUnitario,

            // nuevos campos
            EsPorPeso = dto.EsPorPeso,
            UnidadBase = dto.UnidadBase,
            FactorBase = dto.FactorBase,

            CategoriaId = dto.CategoriaId,
            EstadoProductoId = dto.EstadoProductoId
        };

        _db.Productos.Add(p);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
    }

    // ===========================================================
    // EDITAR (registrando historial de precios)
    // ===========================================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] ProductoUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Datos inválidos", errors = ModelState });

            var p = await _db.Productos.FindAsync(id);
            if (p is null)
                return NotFound(new { message = "Producto no encontrado." });

            if (!string.IsNullOrWhiteSpace(dto.CodigoBarras))
            {
                var exists = await _db.Productos.AnyAsync(x => x.CodigoBarras == dto.CodigoBarras && x.Id != id);
                if (exists) return BadRequest(new { message = "El código de barras ya existe." });
            }

            if (dto.CategoriaId.HasValue && !await _db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
                return BadRequest(new { message = "La categoría no existe." });

            if (dto.EstadoProductoId.HasValue && !await _db.EstadosProducto.AnyAsync(e => e.Id == dto.EstadoProductoId))
                return BadRequest(new { message = "El estado de producto no existe." });

            // Detectar cambio de precio
            decimal precioAnterior = p.PrecioUnitario;
            decimal precioNuevo = dto.PrecioUnitario ?? p.PrecioUnitario;
            bool precioCambio = precioNuevo != p.PrecioUnitario;

            // Actualizar producto
            p.Nombre = dto.Nombre?.Trim() ?? p.Nombre;
            p.Descripcion = dto.Descripcion ?? p.Descripcion;
            p.CodigoBarras = dto.CodigoBarras ?? p.CodigoBarras;
            p.PrecioUnitario = precioNuevo;

            // nuevos campos
            if (dto.EsPorPeso.HasValue) p.EsPorPeso = dto.EsPorPeso.Value;
            if (dto.UnidadBase != null) p.UnidadBase = dto.UnidadBase;
            if (dto.FactorBase.HasValue) p.FactorBase = dto.FactorBase.Value;

            p.CategoriaId = dto.CategoriaId ?? p.CategoriaId;
            p.EstadoProductoId = dto.EstadoProductoId ?? p.EstadoProductoId;

            // Registrar historial de precio si cambió
            if (precioCambio)
            {
                var historial = new HistorialPrecio
                {
                    ProductoId = p.Id,
                    PrecioAnterior = precioAnterior,
                    PrecioNuevo = precioNuevo,
                    FechaCambio = DateTime.UtcNow,
                    Usuario = User.Identity?.Name ?? "Sistema"
                };
                _db.HistorialPrecios.Add(historial);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 ERROR AL EDITAR PRODUCTO: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { message = "Error interno al editar producto", detalle = ex.Message });
        }
    }

    // ===========================================================
    // OBTENER POR ID
    // ===========================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoReadDto>> GetById(int id)
    {
        var p = await _db.Productos
            .AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.EstadoProducto)
            .Include(x => x.Lotes)
            .Where(x => x.Id == id)
            .Select(x => new ProductoReadDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Descripcion = x.Descripcion,
                CodigoBarras = x.CodigoBarras,
                PrecioUnitario = x.PrecioUnitario,

                EsPorPeso = x.EsPorPeso,
                UnidadBase = x.UnidadBase,
                FactorBase = x.FactorBase,

                CategoriaId = x.CategoriaId,
                CategoriaNombre = x.Categoria != null ? x.Categoria.Nombre : null,

                EstadoProductoId = x.EstadoProductoId,
                EstadoProductoNombre = x.EstadoProducto != null ? x.EstadoProducto.Nombre : null,

                StockReal = x.Lotes.Sum(l => l.CantidadActual),

                Categoria = x.Categoria != null
                    ? new CategoriaMini { Id = x.Categoria.Id, Nombre = x.Categoria.Nombre }
                    : null,

                EstadoProducto = x.EstadoProducto != null
                    ? new EstadoProductoMini { Id = x.EstadoProducto.Id, Nombre = x.EstadoProducto.Nombre }
                    : null
            })
            .FirstOrDefaultAsync();

        return p is null ? NotFound() : Ok(p);
    }


    // ===========================================================
    // ELIMINAR
    // ===========================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Disable(int id)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p is null)
            return NotFound(new { message = "Producto no encontrado." });

        p.EstadoProductoId = await _db.EstadosProducto
            .Where(e => e.Nombre == "Inactivo")
            .Select(e => e.Id)
            .FirstAsync();

        await _db.SaveChangesAsync();
        return Ok(new { message = "Producto deshabilitado." });
    }

    [HttpPut("{id:int}/habilitar")]
    public async Task<IActionResult> Habilitar(int id)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p is null)
            return NotFound(new { message = "Producto no encontrado." });

        p.EstadoProductoId = await _db.EstadosProducto
            .Where(e => e.Nombre == "Activo")
            .Select(e => e.Id)
            .FirstAsync();

        await _db.SaveChangesAsync();
        return Ok(new { message = "Producto habilitado." });
    }



    // ===========================================================
    // HISTORIAL DE PRECIOS
    // ===========================================================
    [HttpGet("{id:int}/historial-precios")]
    public async Task<IActionResult> GetHistorialPrecios(int id)
    {
        var existe = await _db.Productos.AnyAsync(x => x.Id == id);
        if (!existe)
            return NotFound(new { message = "Producto no encontrado." });

        var historial = await _db.HistorialPrecios
            .AsNoTracking()
            .Where(h => h.ProductoId == id)
            .OrderByDescending(h => h.FechaCambio)
            .Select(h => new
            {
                h.Id,
                Producto = h.Producto.Nombre,
                h.PrecioAnterior,
                h.PrecioNuevo,
                Fecha = h.FechaCambio,
                h.Usuario
            })
            .ToListAsync();

        return Ok(historial);
    }
}
