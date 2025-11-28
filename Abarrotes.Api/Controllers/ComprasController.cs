using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos;
using Abarrotes.Api.Dtos.Common;
using Abarrotes.Api.Dtos.Compras;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace Abarrotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private const decimal IGV_RATE = 0.18m;

    public ComprasController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // ================================================================
    //  CREAR COMPRA
    // ================================================================
    [HttpPost]
    public async Task<ActionResult<CompraResponseDto>> Crear([FromBody] CompraRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _ctx.Proveedores.AnyAsync(p => p.Id == dto.ProveedorId, ct))
            return BadRequest("El proveedor no existe.");

        // ================================================================
        // 🔥 VALIDAR PRODUCTOS INACTIVOS (AGREGADO)
        // ================================================================
        foreach (var item in dto.Items)
        {
            var prod = await _ctx.Productos
                .AsNoTracking()
                .Where(x => x.Id == item.ProductoId)
                .Select(x => new { x.Id, Estado = x.EstadoProducto!.Nombre })
                .FirstOrDefaultAsync(ct);

            if (prod == null)
                return BadRequest($"El producto {item.ProductoId} no existe.");

            if (prod.Estado == "Inactivo")
                return BadRequest($"No se puede registrar compras del producto '{prod.Id}' porque está INACTIVO.");
        }
        // ================================================================
        // FIN VALIDACIÓN PRODUCTOS INACTIVOS
        // ================================================================

        // Validar existencia de productos
        var productosIds = dto.Items.Select(i => i.ProductoId).Distinct().ToList();
        var productos = await _ctx.Productos
            .Where(p => productosIds.Contains(p.Id))
            .ToListAsync(ct);

        if (productos.Count != productosIds.Count)
            return BadRequest("Hay productos inexistentes.");

        // Validaciones de cada detalle
        foreach (var it in dto.Items)
        {
            if (it.Cantidad <= 0)
                return BadRequest("La cantidad debe ser mayor a cero.");

            if (it.CostoUnitario <= 0)
                return BadRequest("El costo unitario debe ser mayor a cero.");
        }

        // VALIDACIONES SEGÚN EL TIPO DE PRODUCTO
        foreach (var it in dto.Items)
        {
            var producto = productos.First(p => p.Id == it.ProductoId);

            if (producto.EsPorPeso)
            {
                if (it.Cantidad <= 0)
                    return BadRequest($"La cantidad del producto '{producto.Nombre}' debe ser mayor a 0.");
            }
            else
            {
                if (it.Cantidad <= 0 || it.Cantidad != Math.Truncate(it.Cantidad))
                    return BadRequest($"El producto '{producto.Nombre}' solo se compra por unidades enteras.");
            }
        }

        // Calcular totales
        decimal subtotal = dto.Items.Sum(i => decimal.Round(i.Cantidad * i.CostoUnitario, 2));
        decimal igv = dto.AplicaIgv ? decimal.Round(subtotal * IGV_RATE, 2) : 0m;
        decimal total = subtotal + igv;

        using var trx = await _ctx.Database.BeginTransactionAsync(ct);

        try
        {
            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,
                Fecha = dto.Fecha,
                AplicaIgv = dto.AplicaIgv,
                NroComprobante = dto.NroComprobante,
                Observacion = dto.Observacion,
                AfectaInventario = dto.AfectaInventario,
                Subtotal = subtotal,
                Igv = igv,
                Total = total,
                EstadoCompra = dto.AfectaInventario ? "REGISTRADA" : "PENDIENTE"
            };

            _ctx.Compras.Add(compra);
            await _ctx.SaveChangesAsync(ct);

            // Insertar detalles
            var detalles = dto.Items.Select(it => new CompraDetalle
            {
                CompraId = compra.Id,
                ProductoId = it.ProductoId,
                Cantidad = it.Cantidad,
                CostoUnitario = it.CostoUnitario,
                FechaVencimiento = it.FechaVencimiento,
                Subtotal = decimal.Round(it.Cantidad * it.CostoUnitario, 2)
            }).ToList();

            _ctx.ComprasDetalle.AddRange(detalles);
            await _ctx.SaveChangesAsync(ct);

            // CREAR LOTES
            if (dto.AfectaInventario)
            {
                foreach (var it in dto.Items)
                {
                    var lote = new Lote
                    {
                        ProductoId = it.ProductoId,
                        ProveedorId = dto.ProveedorId,
                        FechaIngreso = dto.Fecha,
                        FechaVencimiento = it.FechaVencimiento,
                        CantidadInicial = it.Cantidad,
                        CantidadActual = it.Cantidad,
                        CostoUnitario = it.CostoUnitario,
                        Estado = "ACTIVO",
                        CompraId = compra.Id
                    };

                    _ctx.Lotes.Add(lote);
                }

                await _ctx.SaveChangesAsync(ct);
            }

            await trx.CommitAsync(ct);

            return Ok(new CompraResponseDto
            {
                Id = compra.Id,
                AfectaInventario = compra.AfectaInventario,
                Subtotal = compra.Subtotal,
                Igv = compra.Igv,
                Total = compra.Total,
                Items = detalles.Count
            });
        }
        catch (Exception ex)
        {
            await trx.RollbackAsync(ct);
            return StatusCode(500, $"Error al registrar la compra: {ex.Message}");
        }
    }

    // ================================================================
    // GET: api/Compras/{id}
    // ================================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id, CancellationToken ct)
    {
        var compra = await _ctx.Compras
            .AsNoTracking()
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (compra is null)
            return NotFound();

        return Ok(new
        {
            compra.Id,
            compra.Fecha,
            Proveedor = compra.Proveedor?.Nombre,
            compra.NroComprobante,
            compra.AplicaIgv,
            compra.AfectaInventario,
            compra.EstadoCompra,
            compra.Subtotal,
            compra.Igv,
            compra.Total,
            Items = compra.Detalles.Select(d => new
            {
                d.ProductoId,
                Producto = d.Producto?.Nombre,
                d.Cantidad,
                d.CostoUnitario,
                d.Subtotal,
                d.FechaVencimiento
            })
        });
    }

    // ================================================================
    // LISTAR
    // ================================================================
    [HttpGet]
    public async Task<ActionResult<Paged<CompraListItem>>> Listar([FromQuery] ComprasQuery q, CancellationToken ct)
    {
        var query = _ctx.Compras
            .AsNoTracking()
            .Include(c => c.Proveedor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Desde) && DateTime.TryParse(q.Desde, out var d1))
        {
            var desde = DateTime.SpecifyKind(d1.Date, DateTimeKind.Utc);
            query = query.Where(c => c.Fecha >= desde);
        }

        if (!string.IsNullOrWhiteSpace(q.Hasta) && DateTime.TryParse(q.Hasta, out var d2))
        {
            var hasta = DateTime.SpecifyKind(d2.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(c => c.Fecha < hasta);
        }


        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(c =>
                (c.NroComprobante ?? "").ToLower().Contains(s) ||
                (c.Proveedor!.Nombre ?? "").ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(q.Estado))
        {
            var e = q.Estado.ToUpperInvariant();
            if (e is "REGISTRADA" or "PENDIENTE" or "ANULADA")
                query = query.Where(c => c.EstadoCompra == e);
            else if (e == "VIGENTE")
                query = query.Where(c => c.EstadoCompra != "ANULADA");
        }

        var total = await query.CountAsync(ct);
        var skip = (q.Page - 1) * q.PageSize;

        var items = await query
            .OrderByDescending(c => c.Fecha)
            .Skip(skip).Take(q.PageSize)
            .Select(c => new CompraListItem
            {
                Id = c.Id,
                ComprobanteTipo = c.ComprobanteTipo ?? "TICKET",
                NroComprobante = c.NroComprobante,
                Fecha = c.Fecha,
                Proveedor = c.Proveedor!.Nombre,
                TipoPago = c.TipoPago ?? "CONTADO",
                Total = c.Total,
                Estado = c.EstadoCompra
            })
            .ToListAsync(ct);

        return Ok(new Paged<CompraListItem>
        {
            Page = q.Page,
            PageSize = q.PageSize,
            Total = total,
            Items = items
        });
    }

    // ================================================================
    // ANULAR
    // ================================================================
    [HttpPut("{id:int}/anular")]
    public async Task<IActionResult> Anular(int id, CancellationToken ct)
    {
        var compra = await _ctx.Compras
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (compra is null)
            return NotFound("Compra no encontrada.");

        if (compra.EstadoCompra == "ANULADA")
            return BadRequest("La compra ya está anulada.");

        using var trx = await _ctx.Database.BeginTransactionAsync(ct);

        try
        {
            if (compra.EstadoCompra == "REGISTRADA" && compra.AfectaInventario)
            {
                var lotes = await _ctx.Lotes
                    .Where(l => l.CompraId == compra.Id && l.Estado == "ACTIVO")
                    .ToListAsync(ct);

                foreach (var lote in lotes)
                {
                    lote.Estado = "ANULADO";
                    lote.CantidadActual = 0;
                }

                await _ctx.SaveChangesAsync(ct);
            }

            compra.EstadoCompra = "ANULADA";
            await _ctx.SaveChangesAsync(ct);

            await trx.CommitAsync(ct);
            return Ok(new { ok = true, message = "Compra anulada correctamente." });
        }
        catch (Exception ex)
        {
            await trx.RollbackAsync(ct);
            return StatusCode(500, $"Error al anular la compra: {ex.Message}");
        }
    }
    // ================================================================
    // REPORTE PDF
    // GET: api/Compras/reporte
    // ================================================================
    [HttpGet("reporte")]
    public async Task<IActionResult> Reporte([FromQuery] ComprasQuery q, CancellationToken ct)
    {
        var query = _ctx.Compras
            .AsNoTracking()
            .Include(c => c.Proveedor)
            .AsQueryable();

        // Filtros
        if (!string.IsNullOrWhiteSpace(q.Desde) && DateTime.TryParse(q.Desde, out var d1))
            query = query.Where(c => c.Fecha >= d1.Date);

        if (!string.IsNullOrWhiteSpace(q.Hasta) && DateTime.TryParse(q.Hasta, out var d2))
            query = query.Where(c => c.Fecha < d2.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(c =>
                (c.NroComprobante ?? "").ToLower().Contains(s) ||
                (c.Proveedor!.Nombre ?? "").ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(q.Estado))
        {
            var e = q.Estado.ToUpperInvariant();
            if (e is "REGISTRADA" or "PENDIENTE" or "ANULADA")
                query = query.Where(c => c.EstadoCompra == e);
            else if (e == "VIGENTE")
                query = query.Where(c => c.EstadoCompra != "ANULADA");
        }

        // Obtener items
        var compras = await query
            .OrderByDescending(c => c.Fecha)
            .Select(c => new
            {
                c.Id,
                c.Fecha,
                Proveedor = c.Proveedor!.Nombre,
                c.NroComprobante,
                Tipo = c.ComprobanteTipo ?? "TICKET",
                Pago = c.TipoPago ?? "CONTADO",
                c.Total,
                Estado = c.EstadoCompra
            })
            .ToListAsync(ct);

        // Si no hay datos
        if (!compras.Any())
            return BadRequest("No hay datos para generar el reporte.");

        // Crear documento PDF con QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text("Reporte de Compras").FontSize(20).Bold();
                page.Content().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(60); // Fecha
                        c.RelativeColumn();  // Proveedor
                        c.ConstantColumn(80); // Comprobante
                        c.ConstantColumn(60); // Pago
                        c.ConstantColumn(80); // Total
                        c.ConstantColumn(70); // Estado
                    });

                    // Encabezado
                    t.Header(h =>
                    {
                        h.Cell().Text("Fecha").Bold();
                        h.Cell().Text("Proveedor").Bold();
                        h.Cell().Text("Comprobante").Bold();
                        h.Cell().Text("Pago").Bold();
                        h.Cell().Text("Total").Bold();
                        h.Cell().Text("Estado").Bold();
                    });

                    // Filas
                    foreach (var c in compras)
                    {
                        t.Cell().Text(c.Fecha.ToString("dd/MM/yyyy"));
                        t.Cell().Text(c.Proveedor);
                        t.Cell().Text($"{c.Tipo} {c.NroComprobante}");
                        t.Cell().Text(c.Pago);
                        t.Cell().AlignRight().Text($"S/ {c.Total:F2}");
                        t.Cell().Text(c.Estado);
                    }
                });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return File(pdfBytes, "application/pdf", "reporte_compras.pdf");
    }

}
