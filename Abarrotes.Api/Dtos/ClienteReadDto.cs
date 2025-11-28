using System.ComponentModel.DataAnnotations;

namespace Abarrotes.Api.Dtos
{
    public class ClienteReadDto
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public bool Estado { get; set; }
        // public DateTime FechaRegistro { get; set; } // Puedes incluirla si la usas
    }

    public class ClienteCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(150)]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(150)]
        public string Apellidos { get; set; }

        [Required]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; }

        public string? Direccion { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }
        public bool Estado { get; set; } = true;

    }

    public class ClienteUpdateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(150)]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(150)]
        public string Apellidos { get; set; }

        [Required]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; }

        public string? Direccion { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]

        public string? Email { get; set; }
        public bool Estado { get; set; }
    }
}
