namespace Abarrotes.Api.Dtos.Categorias;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Vigente { get; set; }
}
