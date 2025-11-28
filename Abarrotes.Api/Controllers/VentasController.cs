// ======= INICIO DEL CONTROLADOR ==================================

using Abarrotes.Api.Data;
using Abarrotes.Api.Models;
using Abarrotes.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Vendedor,Admin")]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;
    public VentasController(AppDbContext db) => _db = db;

    // ============================================================
    // POST: api/Ventas  (Idempotente)
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<VentaResponse>> Crear([FromBody] VentaCreate dto, CancellationToken ct)
    {
        // --------- Idempotencia ----------
        if (string.IsNullOrWhiteSpace(dto.RequestKey))
            return BadRequest("Falta RequestKey (GUID único por solicitud).");

        _db.VentaRequests.Add(new VentaRequest { Key = dto.RequestKey! });

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict("Solicitud duplicada (RequestKey repetido).");
        }

        // ======================================================
        // 🔥 VALIDAR PRODUCTOS ACTIVOS (AÑADIDO AQUÍ)
        // ======================================================
        foreach (var d in dto.Detalles)
        {
            var prod = await _db.Productos
                .AsNoTracking()
                .Where(x => x.Id == d.ProductoId)
                .Select(x => new { x.Id, Estado = x.EstadoProducto!.Nombre })
                .FirstOrDefaultAsync(ct);

            if (prod == null)
                return BadRequest($"El producto con ID {d.ProductoId} no existe.");

            if (prod.Estado == "Inactivo")
                return BadRequest($"El producto '{prod.Id}' está DESHABILITADO y no puede venderse.");
        }
        // =======================================================
        // FIN VALIDACIÓN PRODUCTOS INACTIVOS
        // =======================================================

        // --------- Validaciones de entrada ----------
        if (dto.Detalles == null || !dto.Detalles.Any())
            return BadRequest("La venta debe tener al menos un detalle.");

        var estadoPagoId = dto.EstadoPagoId ?? 1;

        if (estadoPagoId == 2 && !dto.ClienteId.HasValue)
            return BadRequest("No se puede registrar una venta PENDIENTE sin cliente asignado.");

        if (dto.MetodoPagoId.HasValue &&
            !await _db.MetodosPago.AnyAsync(m => m.Id == dto.MetodoPagoId.Value, ct))
            return BadRequest("Método de pago inválido.");

        if (dto.ClienteId.HasValue &&
            !await _db.Clientes.AnyAsync(c => c.Id == dto.ClienteId.Value, ct))
            return BadRequest("Cliente inválido.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // Cache de productos (tracked)
            var productosCache = new Dictionary<int, Producto>();

            foreach (var d in dto.Detalles)
            {
                if (!productosCache.TryGetValue(d.ProductoId, out var prod))
                {
                    prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == d.ProductoId, ct)
                           ?? throw new InvalidOperationException($"Producto {d.ProductoId} no existe.");
                    productosCache[d.ProductoId] = prod;
                }

                // Validación unidades o peso
                if (prod.EsPorPeso)
                {
                    if (d.Cantidad <= 0m)
                        return BadRequest($"Cantidad inválida (en gramos) para '{prod.Nombre}'.");
                }
                else
                {
                    if (d.Cantidad <= 0m || d.Cantidad != Math.Truncate(d.Cantidad))
                        return BadRequest($"El producto '{prod.Nombre}' solo se vende en unidades enteras.");
                }

                // Validar stock
                var disponible = await _db.Lotes
                    .Where(l => l.ProductoId == d.ProductoId && l.CantidadActual > 0m)
                    .SumAsync(l => (decimal?)l.CantidadActual, ct) ?? 0m;

                if (disponible < d.Cantidad)
                    return BadRequest($"Stock insuficiente para '{prod.Nombre}'. Disponible: {disponible}, requerido: {d.Cantidad}.");
            }

            // Crear venta
            var venta = new Venta
            {
                Fecha = DateTime.UtcNow,
                MetodoPagoId = dto.MetodoPagoId,
                EstadoPagoId = estadoPagoId,
                ClienteId = dto.ClienteId,
                TipoComprobante = dto.TipoComprobante,
                Detalleventa = new List<DetalleVenta>()
            };

            var auditoria = new List<ConsumoLote>();

            // FIFO
            foreach (var d in dto.Detalles)
            {
                var producto = productosCache[d.ProductoId];
                var precio = d.PrecioUnitario > 0 ? d.PrecioUnitario : producto.PrecioUnitario;

                var lotes = await _db.Lotes
                    .Where(l => l.ProductoId == d.ProductoId && l.CantidadActual > 0m)
                    .OrderBy(l => l.FechaIngreso)
                    .ThenBy(l => l.Id)
                    .ToListAsync(ct);

                decimal remaining = d.Cantidad;

                foreach (var lote in lotes)
                {
                    if (remaining <= 0m) break;

                    var take = Math.Min(remaining, lote.CantidadActual);
                    remaining -= take;

                    lote.CantidadActual -= take;
                    _db.Entry(lote).Property(x => x.CantidadActual).IsModified = true;

                    auditoria.Add(new ConsumoLote
                    {
                        Venta = venta,
                        LoteId = lote.Id,
                        Cantidad = take
                    });
                }

                if (remaining > 0)
                    return BadRequest($"Stock insuficiente al confirmar para producto {d.ProductoId}.");

                venta.Detalleventa.Add(new DetalleVenta
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = decimal.Round(d.Cantidad * precio, 2)
                });
            }

            // 🔥 RE-CALCULAR total SIEMPRE basado en los subtotales agregados
            venta.Total = decimal.Round(
                venta.Detalleventa.Sum(d => d.Subtotal),
                2,
                MidpointRounding.AwayFromZero
            );

            // 🔥 Si es venta pendiente, MontoPendiente = Total
            venta.MontoPendiente = estadoPagoId == 2 ? venta.Total : 0m;

            _db.Ventas.Add(venta);
            _db.ConsumosLote.AddRange(auditoria);
            await _db.SaveChangesAsync(ct);

            // Comprobantes
            if (dto.TipoComprobante?.ToLower() == "boleta")
            {
                var numero = await NextNumeroBoleta(ct);
                _db.Boletas.Add(new Boleta { VentaId = venta.Id, NumeroBoleta = numero, FechaEmision = DateTime.UtcNow });
                venta.ComprobanteNumero = numero;
            }
            else if (dto.TipoComprobante?.ToLower() == "factura")
            {
                var numero = await NextNumeroFactura(ct);
                _db.Facturas.Add(new Factura { VentaId = venta.Id, NumeroFactura = numero, FechaEmision = DateTime.UtcNow, Estado = "emitida" });
                venta.ComprobanteNumero = numero;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, new VentaResponse
            {
                Id = venta.Id,
                TipoComprobante = venta.TipoComprobante!,
                ComprobanteNumero = venta.ComprobanteNumero,
                Total = venta.Total,
                Fecha = venta.Fecha
            });
        }
        catch
        {
            try { await tx.RollbackAsync(ct); } catch { }
            throw;
        }
    }

    // ============================================================
    // GET: api/Ventas/{id}
    // ============================================================
    [HttpGet("{id}")]
    public async Task<ActionResult<VentaResponse>> GetById(int id, CancellationToken ct)
    {
        var v = await _db.Ventas
            .Include(x => x.Boleta)
            .Include(x => x.Factura)
            .Include(x => x.Detalleventa)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (v == null) return NotFound();

        return Ok(new VentaResponse
        {
            Id = v.Id,
            TipoComprobante = v.TipoComprobante ?? "",
            ComprobanteNumero = v.ComprobanteNumero,
            Total = v.Total,
            Fecha = v.Fecha
        });
    }

    // ============================================================
    // Secuencias comprobantes
    // ============================================================
    private async Task<string> NextNumeroBoleta(CancellationToken ct)
    {
        var last = await _db.Boletas
            .OrderByDescending(b => b.Id)
            .Select(b => b.NumeroBoleta)
            .FirstOrDefaultAsync(ct);

        _ = int.TryParse(last, out int n);
        return (n + 1).ToString("D8");
    }

    private async Task<string> NextNumeroFactura(CancellationToken ct)
    {
        var last = await _db.Facturas
            .OrderByDescending(f => f.Id)
            .Select(f => f.NumeroFactura)
            .FirstOrDefaultAsync(ct);

        _ = int.TryParse(last, out int n);
        return (n + 1).ToString("D8");
    }

    [HttpGet("por-fecha")]
    public async Task<ActionResult<IEnumerable<VentaListadoDto>>> VentasPorFecha(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fin,
        CancellationToken ct)
    {
        if (inicio == null || fin == null)
            return BadRequest("Debe proporcionar fecha inicio y fecha fin.");

        // Normalizar → convertir a UTC obligatorio
        var ini = DateTime.SpecifyKind(inicio.Value.Date, DateTimeKind.Utc);
        var fi = DateTime.SpecifyKind(fin.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var ventas = await _db.Ventas
            .AsNoTracking()
            .Include(v => v.MetodoPago)
            .Include(v => v.Cliente)
            .Include(v => v.EstadoPago)
            .Where(v => v.Fecha >= ini && v.Fecha <= fi)
            .OrderBy(v => v.Fecha)
            .Select(v => new VentaListadoDto
            {
                Id = v.Id,
                Fecha = v.Fecha,
                TipoComprobante = v.TipoComprobante!,
                ComprobanteNumero = v.ComprobanteNumero!,
                MetodoPago = v.MetodoPago!.Nombre,
                Total = v.Total,
                Cliente = v.Cliente != null ? v.Cliente.Nombres : "Público General",
                EstadoPago = v.EstadoPago != null ? v.EstadoPago.Nombre : "Desconocido",
                MontoPendiente = v.MontoPendiente   // ⭐ AQUÍ
            })
            .ToListAsync(ct);

        return Ok(ventas);
    }

    // =========================
    // GET: api/Ventas/por-fecha/todas
    // =========================
    [HttpGet("por-fecha/todas")]
    public async Task<ActionResult<IEnumerable<VentaListadoDto>>> Todas(CancellationToken ct)
    {
        var ventas = await _db.Ventas
            .AsNoTracking()
            .Include(v => v.MetodoPago)
            .Include(v => v.Cliente)
            .Include(v => v.EstadoPago)
            .OrderBy(v => v.Fecha)
            .Select(v => new VentaListadoDto
            {
                Id = v.Id,
                Fecha = v.Fecha,
                TipoComprobante = v.TipoComprobante!,
                ComprobanteNumero = v.ComprobanteNumero!,
                MetodoPago = v.MetodoPago!.Nombre,
                Total = v.Total,
                Cliente = v.Cliente != null ? v.Cliente.Nombres : "Público General",
                EstadoPago = v.EstadoPago != null ? v.EstadoPago.Nombre : "Desconocido",
                MontoPendiente = v.MontoPendiente   // ⭐ AQUÍ

            })
            .ToListAsync(ct);

        return Ok(ventas);
    }

    [HttpGet("pdf/{id}")]
    public async Task<IActionResult> GenerarPdf(int id)
    {
        var venta = await _db.Ventas
            .Include(v => v.Detalleventa)
            .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
            return NotFound("Venta no encontrada");

        // Selección de formato según comprobante
        byte[] pdfBytes = venta.TipoComprobante?.ToLower() switch
        {
            "ticket" => GenerarTicketPDF(venta),
            "boleta" => GenerarBoletaPDF(venta),
            "factura" => GenerarFacturaPDF(venta),
            _ => GenerarTicketPDF(venta) // ✔ por defecto ticket
        };

        return File(pdfBytes, "application/pdf", $"{venta.TipoComprobante}_{venta.Id}.pdf");
    }

    private byte[] GenerarTicketPDF(Venta venta)
    {
        using var ms = new MemoryStream();

        var pageSize = new iTextSharp.text.Rectangle(226, 600);
        var doc = new Document(pageSize, 5, 5, 5, 5);
        PdfWriter.GetInstance(doc, ms);

        doc.Open();

        var title = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        var normal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

        doc.Add(new Paragraph("COMPROBANTE DE VENTA", title) { Alignment = 1 });
        doc.Add(new Paragraph("------------------------------", normal));

        doc.Add(new Paragraph($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}", normal));
        doc.Add(new Paragraph($"Tipo: {venta.TipoComprobante}", normal));
        doc.Add(new Paragraph($"N°: {venta.ComprobanteNumero}", normal));
        doc.Add(new Paragraph(" ", normal));

        PdfPTable table = new PdfPTable(3);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 40, 20, 40 });
        table.AddCell("Producto");
        table.AddCell("Cant");
        table.AddCell("Subt");

        foreach (var d in venta.Detalleventa)
        {
            table.AddCell(d.Producto.Nombre);
            table.AddCell($"{d.Cantidad}");
            table.AddCell($"S/ {d.Subtotal:0.00}");
        }

        doc.Add(table);

        doc.Add(new Paragraph("------------------------------", normal));
        doc.Add(new Paragraph($"TOTAL: S/ {venta.Total:0.00}", bold));

        doc.Close();
        return ms.ToArray();
    }

    private byte[] GenerarBoletaPDF(Venta venta)
    {
        using var ms = new MemoryStream();

        var doc = new Document(PageSize.A5, 20, 20, 20, 20);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        var title = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var normal = FontFactory.GetFont(FontFactory.HELVETICA, 11);
        var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

        doc.Add(new Paragraph("BOLETA DE VENTA", title) { Alignment = Element.ALIGN_CENTER });
        doc.Add(new Paragraph($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}", normal));
        doc.Add(new Paragraph($"Número: {venta.ComprobanteNumero}", normal));
        doc.Add(new Paragraph(" ", normal));

        PdfPTable table = new PdfPTable(4);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 40, 20, 20, 20 });
        table.AddCell("Producto");
        table.AddCell("Cant.");
        table.AddCell("Precio");
        table.AddCell("Subt.");

        foreach (var d in venta.Detalleventa)
        {
            table.AddCell(d.Producto.Nombre);
            table.AddCell($"{d.Cantidad}");
            table.AddCell($"S/ {d.PrecioUnitario:0.00}");
            table.AddCell($"S/ {d.Subtotal:0.00}");
        }

        doc.Add(table);

        doc.Add(new Paragraph("\nTOTAL A PAGAR", bold));
        doc.Add(new Paragraph($"S/ {venta.Total:0.00}", bold));

        doc.Close();
        return ms.ToArray();
    }

    private byte[] GenerarFacturaPDF(Venta venta)
    {
        using var ms = new MemoryStream();

        var doc = new Document(PageSize.A4, 25, 25, 25, 25);
        PdfWriter.GetInstance(doc, ms);
        doc.Open();

        var title = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
        var normal = FontFactory.GetFont(FontFactory.HELVETICA, 12);
        var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

        doc.Add(new Paragraph("FACTURA ELECTRÓNICA", title) { Alignment = Element.ALIGN_CENTER });
        doc.Add(new Paragraph("\n"));

        doc.Add(new Paragraph($"Fecha de emisión: {venta.Fecha:dd/MM/yyyy HH:mm}", normal));
        doc.Add(new Paragraph($"Número: {venta.ComprobanteNumero}", normal));
        doc.Add(new Paragraph("\n"));

        PdfPTable table = new PdfPTable(4);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 40, 15, 15, 30 });

        table.AddCell("Producto");
        table.AddCell("Cantidad");
        table.AddCell("Precio");
        table.AddCell("Subtotal");

        foreach (var d in venta.Detalleventa)
        {
            table.AddCell(d.Producto.Nombre);
            table.AddCell($"{d.Cantidad}");
            table.AddCell($"S/ {d.PrecioUnitario:0.00}");
            table.AddCell($"S/ {d.Subtotal:0.00}");
        }

        doc.Add(table);

        doc.Add(new Paragraph("\nTOTAL A PAGAR", bold));
        doc.Add(new Paragraph($"S/ {venta.Total:0.00}", title));

        doc.Close();
        return ms.ToArray();
    }
    // ============================================================
    // GET: api/Ventas/{id}/detalle   -> Detalle completo de la venta
    // ============================================================
    [HttpGet("{id}/detalle")]
    public async Task<ActionResult<VentaDetalleViewDto>> Detalle(int id, CancellationToken ct)
    {
        const decimal IGV_RATE = 0.18m; // 18%

        var v = await _db.Ventas
            .AsNoTracking()
            .Include(x => x.Detalleventa)
                .ThenInclude(d => d.Producto)
            .Include(x => x.Cliente)
            .Include(x => x.MetodoPago)
            .Include(x => x.EstadoPago)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (v == null)
            return NotFound();

        var total = v.Total;
        var subtotal = decimal.Round(total / (1 + IGV_RATE), 2);
        var igv = total - subtotal;

        var dto = new VentaDetalleViewDto
        {
            Id = v.Id,
            Fecha = v.Fecha,
            TipoComprobante = v.TipoComprobante ?? "",
            ComprobanteNumero = v.ComprobanteNumero,
            Cliente = v.Cliente != null ? v.Cliente.Nombres : "Público General",
            MetodoPago = v.MetodoPago != null ? v.MetodoPago.Nombre : "",
            EstadoPago = v.EstadoPago != null ? v.EstadoPago.Nombre : "",
            Total = total,
            Subtotal = subtotal,
            Igv = igv,
            Lineas = v.Detalleventa.Select(d => new VentaDetalleLineaDto
            {
                Producto = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };

        return Ok(dto);
    }

    // ============================================================
    // PUT: api/Ventas/{id}/estado   -> Cambiar estado de pago
    // ============================================================
    [HttpPut("{id}/estado")]
    public async Task<ActionResult> CambiarEstado(
        int id,
        [FromBody] VentaEstadoUpdateDto dto,
        CancellationToken ct)
    {
        if (dto == null || dto.EstadoPagoId is < 1 or > 3)
            return BadRequest("EstadoPagoId debe ser 1=pagado, 2=pendiente o 3=anulado.");

        var venta = await _db.Ventas
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (venta == null)
            return NotFound("Venta no encontrada.");

        // No permitir salir de ANULADO
        if (venta.EstadoPagoId == 3 && dto.EstadoPagoId != 3)
            return BadRequest("No se puede cambiar una venta ANULADA a otro estado.");

        // Si no cambia nada, terminar
        if (venta.EstadoPagoId == dto.EstadoPagoId)
            return NoContent();

        // ⭐ REGLA: si se marca como PENDIENTE, debe tener cliente
        if (dto.EstadoPagoId == 2 && venta.ClienteId == null)
            return BadRequest("No se puede marcar como PENDIENTE una venta sin cliente.");

        // Si va a ANULADO, revertir stock (FIFO inverso)
        if (dto.EstadoPagoId == 3)
        {
            // Traer consumos y lotes asociados
            var consumos = await _db.ConsumosLote
                .Include(c => c.Lote)
                .Where(c => c.VentaId == id)
                .ToListAsync(ct);

            foreach (var c in consumos)
            {
                if (c.Lote == null) continue;

                c.Lote.CantidadActual += c.Cantidad;
                _db.Entry(c.Lote).Property(x => x.CantidadActual).IsModified = true;
            }

            // ⭐ Venta anulada nunca debe tener pendiente
            venta.MontoPendiente = 0m;
        }

        // ⭐ Ajustar MontoPendiente según nuevo estado
        if (dto.EstadoPagoId == 1)         // Pagado
        {
            venta.MontoPendiente = 0m;
        }
        else if (dto.EstadoPagoId == 2)    // Pendiente
        {
            if (venta.MontoPendiente <= 0)
                venta.MontoPendiente = venta.Total;
        }

        venta.EstadoPagoId = dto.EstadoPagoId;
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ============================================================
    // GET: api/Ventas/pendientes/cliente/{clienteId}
    // ============================================================
    [HttpGet("pendientes/cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<VentaListadoDto>>> PendientesPorCliente(
        int clienteId,
        CancellationToken ct)
    {
        var existe = await _db.Clientes.AnyAsync(c => c.Id == clienteId, ct);
        if (!existe)
            return NotFound("Cliente no encontrado.");

        var ventas = await _db.Ventas
            .AsNoTracking()
            .Include(v => v.MetodoPago)
            .Include(v => v.Cliente)
            .Include(v => v.EstadoPago)
            .Where(v =>
                v.ClienteId == clienteId &&
                v.EstadoPagoId == 2 &&           // Pendiente
                v.MontoPendiente > 0)
            .OrderBy(v => v.Fecha)
            .ThenBy(v => v.Id)
            .Select(v => new VentaListadoDto
            {
                Id = v.Id,
                Fecha = v.Fecha,
                TipoComprobante = v.TipoComprobante!,
                ComprobanteNumero = v.ComprobanteNumero!,
                MetodoPago = v.MetodoPago!.Nombre,
                Total = v.Total,
                Cliente = v.Cliente != null ? v.Cliente.Nombres : "Público General",
                EstadoPago = v.EstadoPago != null ? v.EstadoPago.Nombre : "Desconocido",
                MontoPendiente = v.MontoPendiente
            })
            .ToListAsync(ct);

        return Ok(ventas);
    }

    // ============================================================
    // POST: api/Ventas/pendientes/pagar
    // ============================================================
    [HttpPost("pendientes/pagar")]
    public async Task<ActionResult<VentaPagoPendienteResultDto>> PagarPendientes(
        [FromBody] VentaPagoPendienteRequestDto dto,
        CancellationToken ct)
    {
        if (dto.Monto <= 0)
            return BadRequest("El monto a pagar debe ser mayor que cero.");

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.Id == dto.ClienteId, ct);

        if (cliente == null)
            return NotFound("Cliente no encontrado.");

        var ventas = await _db.Ventas
            .Where(v =>
                v.ClienteId == dto.ClienteId &&
                v.EstadoPagoId == 2 &&
                v.MontoPendiente > 0)
            .OrderBy(v => v.Fecha)
            .ThenBy(v => v.Id)
            .ToListAsync(ct);

        if (!ventas.Any())
            return BadRequest("El cliente no tiene ventas pendientes.");

        decimal restante = dto.Monto;
        var pagadas = new List<int>();
        var parciales = new List<int>();

        foreach (var v in ventas)
        {
            if (restante <= 0) break;

            if (restante >= v.MontoPendiente)
            {
                // Se paga toda la venta
                restante -= v.MontoPendiente;
                v.MontoPendiente = 0m;
                v.EstadoPagoId = 1; // Pagado
                pagadas.Add(v.Id);
            }
            else
            {
                // Solo se paga una parte de esta venta
                v.MontoPendiente -= restante;
                restante = 0m;
                parciales.Add(v.Id);
                break;
            }
        }

        await _db.SaveChangesAsync(ct);

        var result = new VentaPagoPendienteResultDto
        {
            ClienteId = dto.ClienteId,
            MontoOriginal = dto.Monto,
            MontoUsado = dto.Monto - restante,
            MontoSobrante = restante,
            VentasPagadas = pagadas,
            VentasParcialmentePagadas = parciales
        };

        return Ok(result);
    }

}
