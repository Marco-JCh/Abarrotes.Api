using System;

namespace Abarrotes.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    // nombre de usuario para login
    public string UsuarioLogin { get; set; } = null!;

    // nombre real de la persona
    public string Nombres { get; set; } = null!;

    public string? Email { get; set; }

    // hash (BCrypt) — NO guardes contraseñas en texto plano
    public string PasswordHash { get; set; } = null!;

    public string Rol { get; set; } = "Vendedor";

    public bool EstaActivo { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }
}
