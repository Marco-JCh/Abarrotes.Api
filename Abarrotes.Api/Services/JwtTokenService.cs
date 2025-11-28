using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Abarrotes.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Abarrotes.Api.Services;

public interface IJwtTokenService
{
    (string token, DateTime expiraUtc) Generate(Usuario user);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _cfg;

    public JwtTokenService(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public (string token, DateTime expiraUtc) Generate(Usuario user)
    {
        var key = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");
        var issuer = _cfg["Jwt:Issuer"] ?? "Abarrotes.Api";
        var audience = _cfg["Jwt:Audience"] ?? "Abarrotes.Web";
        var minutes = int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 60;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("usuario", user.UsuarioLogin),
            new("nombres", user.Nombres),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(minutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, expires);
    }
}
