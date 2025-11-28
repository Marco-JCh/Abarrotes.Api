using Abarrotes.Api.Data;
using Abarrotes.Api.Dtos;
using Abarrotes.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions; // Necesario para validar el email manualmente

namespace Abarrotes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Ventas,Admin")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ClientesController(AppDbContext db) => _db = db;

        // ====== LISTAR TODOS LOS CLIENTES ======
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteReadDto>>> GetAll([FromQuery] string? filtro)
        {
            var query = _db.Clientes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                string filtroMin = filtro.ToLower().Trim();
                query = query.Where(c =>
                    c.Nombres.ToLower().Contains(filtroMin) ||
                    c.Apellidos.ToLower().Contains(filtroMin) ||
                    c.NumeroDocumento.Contains(filtro)
                );
            }

            var items = await query
                .Select(c => new ClienteReadDto
                {
                    Id = c.Id,
                    Nombres = c.Nombres,
                    Apellidos = c.Apellidos,
                    TipoDocumento = c.TipoDocumento,
                    NumeroDocumento = c.NumeroDocumento,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Direccion = c.Direccion,
                    Estado = c.Vigente
                })
                .OrderBy(x => x.Apellidos).ThenBy(x => x.Nombres)
                .ToListAsync();

            return Ok(items);
        }

        // ====== OBTENER UN CLIENTE POR ID ======
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteReadDto>> GetById(int id)
        {
            var c = await _db.Clientes
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ClienteReadDto
                {
                    Id = x.Id,
                    Nombres = x.Nombres,
                    Apellidos = x.Apellidos,
                    TipoDocumento = x.TipoDocumento,
                    NumeroDocumento = x.NumeroDocumento,
                    Telefono = x.Telefono,
                    Email = x.Email,
                    Direccion = x.Direccion,
                    Estado = x.Vigente
                })
                .FirstOrDefaultAsync();

            return c is null ? NotFound(new { message = "Cliente no encontrado." }) : Ok(c);
        }

        // ====== MÉTODOS PRIVADOS DE AYUDA ======

        // Limpia el email (convierte "" en null) y valida el dominio
        private string? ValidarYLimpiarEmail(string? email, out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(email)) return null; // Si está vacío, devuelve null (válido)

            email = email.Trim();
            // Validación manual de Gmail/Hotmail
            if (!Regex.IsMatch(email, @"^.+@(gmail\.com|hotmail\.com)$", RegexOptions.IgnoreCase))
            {
                error = "El email debe ser @gmail.com o @hotmail.com";
                return null;
            }
            return email;
        }

        // ====== CREAR UN NUEVO CLIENTE ======
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Datos inválidos", errors = ModelState });

            // 1. Limpiar y Validar Email
            string? emailLimpio = ValidarYLimpiarEmail(dto.Email, out string? errorEmail);
            if (errorEmail != null) return BadRequest(new { message = errorEmail });

            // 2. Validar DNI/RUC
            if (dto.TipoDocumento == "DNI" && dto.NumeroDocumento.Length != 8)
                return BadRequest(new { message = "El DNI debe tener 8 dígitos." });
            if (dto.TipoDocumento == "RUC" && dto.NumeroDocumento.Length != 11)
                return BadRequest(new { message = "El RUC debe tener 11 dígitos." });

            // 3. Validar Duplicados
            if (await _db.Clientes.AnyAsync(c => c.NumeroDocumento == dto.NumeroDocumento))
                return Conflict(new { message = "El número de documento ya está registrado." });

            if (emailLimpio != null && await _db.Clientes.AnyAsync(c => c.Email != null && c.Email.ToLower() == emailLimpio.ToLower()))
                return Conflict(new { message = "El correo electrónico ya está registrado." });

            var nuevoCliente = new Cliente
            {
                Nombres = dto.Nombres.Trim(),
                Apellidos = dto.Apellidos.Trim(),
                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                Telefono = dto.Telefono,
                Email = emailLimpio, // Usamos la versión limpia
                Direccion = dto.Direccion,
                Vigente = dto.Estado
            };

            _db.Clientes.Add(nuevoCliente);
            await _db.SaveChangesAsync();

            var response = new ClienteReadDto
            {
                Id = nuevoCliente.Id,
                Nombres = nuevoCliente.Nombres,
                Apellidos = nuevoCliente.Apellidos,
                Estado = nuevoCliente.Vigente
            };
            return CreatedAtAction(nameof(GetById), new { id = nuevoCliente.Id }, response);
        }

        // ====== EDITAR UN CLIENTE EXISTENTE ======
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Datos inválidos", errors = ModelState });

            // 1. Limpiar y Validar Email
            string? emailLimpio = ValidarYLimpiarEmail(dto.Email, out string? errorEmail);
            if (errorEmail != null) return BadRequest(new { message = errorEmail });

            // 2. Validar DNI/RUC
            if (dto.TipoDocumento == "DNI" && dto.NumeroDocumento.Length != 8)
                return BadRequest(new { message = "El DNI debe tener 8 dígitos." });
            if (dto.TipoDocumento == "RUC" && dto.NumeroDocumento.Length != 11)
                return BadRequest(new { message = "El RUC debe tener 11 dígitos." });

            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente is null) return NotFound(new { message = "Cliente no encontrado." });

            // 3. Validar Duplicados
            if (await _db.Clientes.AnyAsync(x => x.NumeroDocumento == dto.NumeroDocumento && x.Id != id))
                return Conflict(new { message = "El número de documento ya pertenece a otro cliente." });

            if (emailLimpio != null && await _db.Clientes.AnyAsync(x => x.Email != null && x.Email.ToLower() == emailLimpio.ToLower() && x.Id != id))
                return Conflict(new { message = "El correo electrónico ya pertenece a otro cliente." });

            cliente.Nombres = dto.Nombres.Trim();
            cliente.Apellidos = dto.Apellidos.Trim();
            cliente.TipoDocumento = dto.TipoDocumento;
            cliente.NumeroDocumento = dto.NumeroDocumento;
            cliente.Telefono = dto.Telefono;
            cliente.Email = emailLimpio; // Usamos la versión limpia
            cliente.Direccion = dto.Direccion;
            cliente.Vigente = dto.Estado;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ====== ELIMINAR UN CLIENTE ======
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _db.Clientes.FindAsync(id);
            if (cliente is null) return NotFound(new { message = "Cliente no encontrado." });

            _db.Clientes.Remove(cliente);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ====== VERIFICAR EXISTENCIA (PARA FRONTEND) ======
        [HttpGet("existe/{numero}")]
        public async Task<ActionResult<bool>> VerificarExistencia(string numero)
        {
            return Ok(await _db.Clientes.AnyAsync(c => c.NumeroDocumento == numero));
        }
    }
}