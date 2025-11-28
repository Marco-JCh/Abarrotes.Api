namespace Abarrotes.Api.Dtos.Auth;
public class RegisterDto
{
    public string Usuario { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string? Email { get; set; }
    public string Password { get; set; } = null!;
}