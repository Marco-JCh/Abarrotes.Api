namespace Abarrotes.Api.Dtos
{
    // Body para crear lote
    public record LoteCreateDto(
        int ProductoId,
        int? ProveedorId,
        DateTime? FechaIngreso,
        DateTime? FechaVencimiento,
        decimal Cantidad,
        decimal CostoUnitario
    );

    // Respuesta del API
    public record LoteResponseDto(
        int Id,
        int ProductoId,
        int? ProveedorId,
        DateTime FechaIngreso,
        DateTime? FechaVencimiento,
        decimal CantidadInicial,
        decimal CantidadActual,
        decimal CostoUnitario
    );
}
