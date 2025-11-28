using System.Security.Claims;
using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos;
using Abarrotes.Api.Models;
using Abarrotes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abarrotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthController(AppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    // ============================
    // POST: api/auth/register
    // Solo Admin
    // ============================
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body requerido.");
        if (string.IsNullOrWhiteSpace(dto.UsuarioLogin) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Usuario y contraseña son requeridos.");
        if (string.IsNullOrWhiteSpace(dto.Nombres))
            return BadRequest("Nombres es requerido.");

        // normalizo usuario para evitar duplicados por case
        var userLoginNorm = dto.UsuarioLogin.Trim().ToLowerInvariant();

        var exists = await _db.Usuarios
            .AnyAsync(x => x.UsuarioLogin.ToLower() == userLoginNorm, ct);
        if (exists) return Conflict("Usuario ya existe.");

        var rol = (dto.Rol ?? "Vendedor").Trim();
        if (!new[] { "Admin", "Vendedor", "Inventario" }.Contains(rol))
            return BadRequest("Rol inválido.");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailNorm = dto.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Usuarios
                .AnyAsync(x => x.Email != null && x.Email.ToLower() == emailNorm, ct);
            if (emailExists) return Conflict("Ya existe un usuario con ese email.");
        }

        var user = new Usuario
        {
            UsuarioLogin = userLoginNorm,
            Nombres = dto.Nombres.Trim(),
            Email = dto.Email?.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            EstaActivo = true,
            CreatedUtc = DateTime.UtcNow,
            Rol = rol
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync(ct);

        var (token, exp) = _jwt.Generate(user);
        return Ok(new AuthResponseDto
        {
            Id = user.Id,
            UsuarioLogin = user.UsuarioLogin,
            Nombres = user.Nombres,
            Token = token,
            ExpiraUtc = exp,
            Rol = user.Rol
        });
    }

    // ============================
    // POST: api/auth/login
    // ============================
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        if (dto is null) return BadRequest("Body requerido.");
        if (string.IsNullOrWhiteSpace(dto.UsuarioLogin) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Usuario y contraseña son requeridos.");

        var userLoginNorm = dto.UsuarioLogin.Trim().ToLowerInvariant();

        var user = await _db.Usuarios
            .FirstOrDefaultAsync(x => x.UsuarioLogin.ToLower() == userLoginNorm, ct);

        if (user is null || !user.EstaActivo)
            return Unauthorized("Usuario o contraseña inválidos.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Usuario o contraseña inválidos.");

        user.LastLoginUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var (token, exp) = _jwt.Generate(user);
        return Ok(new AuthResponseDto
        {
            Id = user.Id,
            UsuarioLogin = user.UsuarioLogin,
            Nombres = user.Nombres,
            Token = token,
            ExpiraUtc = exp,
            Rol = user.Rol
        });
    }

    // ============================
    // GET: api/auth/me  (requiere Bearer)
    // ============================
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> Me(CancellationToken ct)
    {
        // el claim "sub" viene del token (id del usuario)
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) // a veces mapea aquí
                  ?? User.FindFirstValue("sub");                  // o aquí (JwtRegisteredClaimNames.Sub)

        if (!int.TryParse(sub, out var userId))
            return Unauthorized();

        var u = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (u is null) return Unauthorized();

        return Ok(new
        {
            u.Id,
            u.UsuarioLogin,
            u.Nombres,
            u.Email,
            u.EstaActivo,
            u.LastLoginUtc,
            u.Rol
        });
    }

    // =======================================
    // POST: api/auth/change-password (Bearer)
    // =======================================
    public class ChangePasswordDto
    {
        public string Actual { get; set; } = null!;
        public string Nueva { get; set; } = null!;
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Actual) || string.IsNullOrWhiteSpace(dto.Nueva))
            return BadRequest("Campos requeridos.");

        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(sub, out var userId)) return Unauthorized();

        var user = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(dto.Actual, user.PasswordHash))
            return BadRequest("La contraseña actual no es válida.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Nueva);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // =======================================
    // Gestión de usuarios (solo Admin)
    // =======================================

    // GET: api/auth/users
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers(CancellationToken ct)
    {
        var users = await _db.Usuarios
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.UsuarioLogin, x.Nombres, x.Email, x.Rol, x.EstaActivo })
            .ToListAsync(ct);

        return Ok(users);
    }

    // PUT: api/auth/users/{id}/role
    [HttpPut("users/{id}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateRole(int id, [FromBody] string rol, CancellationToken ct)
    {
        if (!new[] { "Admin", "Vendedor", "Inventario" }.Contains(rol))
            return BadRequest("Rol inválido.");

        var user = await _db.Usuarios.FindAsync(new object[] { id }, ct);
        if (user is null) return NotFound();

        user.Rol = rol;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE: api/auth/users/{id}
    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(int id, CancellationToken ct)
    {
        var user = await _db.Usuarios.FindAsync(new object[] { id }, ct);
        if (user is null) return NotFound();

        _db.Usuarios.Remove(user);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
