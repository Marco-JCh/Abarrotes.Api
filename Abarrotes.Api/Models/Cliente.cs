using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abarrotes.Api.Models
{
    [Table("clientes")] // Le dice a EF que esta clase se mapea a la tabla "clientes"
    public class Cliente
    {
        [Key] // Le dice que esta es la llave primaria
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombres { get; set; }

        [Required]
        [MaxLength(150)]
        public string Apellidos { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoDocumento { get; set; }

        [Required]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; }

        public string? Direccion { get; set; } // El '?' permite valores nulos

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }
        // --- AÑADE ESTO ---
        [Required]
        [Column("vigente")]
        public bool Vigente { get; set; } = true; // Por defecto es 'true' (Activo)
        // --- FIN DE LA ADICIÓN ---

        // Esta columna no estaba en tu última foto de la BD.
        // Si la tienes, descomenta la línea de abajo.
        // [Column("fecha_registro")]
        // public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}