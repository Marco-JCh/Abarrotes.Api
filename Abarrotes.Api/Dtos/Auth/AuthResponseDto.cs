namespace Abarrotes.Api.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime Expira { get; set; }
    public string Usuario { get; set; } = null!;
    public string Nombres { get; set; } = null!;
}