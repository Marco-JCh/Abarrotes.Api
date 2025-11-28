public record ProveedorDto(
    int Id, 
    string Nombre, 
    string? Ruc,
    string? Direccion, 
    string? Telefono, 
    string? Email, 
    bool Activo // ✅ asegúrate de incluir esto
);

public class ProveedorCreateDto
{
    public string Nombre { get; set; } = null!;
    public string? Ruc { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}

public class ProveedorUpdateDto : ProveedorCreateDto { }

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}
