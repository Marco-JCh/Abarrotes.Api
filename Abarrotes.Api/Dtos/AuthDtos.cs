namespace Abarrotes.Api.Dtos;

public class LoginRequestDto
{
    public string UsuarioLogin { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequestDto
{
    public string UsuarioLogin { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Email { get; set; }
    public string Password { get; set; } = null!;
    public string Rol { get; set; } = "Vendedor"; // Admin | Vendedor | Inventario

}

public class AuthResponseDto
{
    public int Id { get; set; }
    public string UsuarioLogin { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiraUtc { get; set; }
    public string Rol { get; set; } = null!;

}
