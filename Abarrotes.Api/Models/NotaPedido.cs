using System.ComponentModel.DataAnnotations;

namespace Abarrotes.Api.Models;

public class NotaPedido
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required, StringLength(20)]
    public string Estado { get; set; } = "pendiente"; // 'pendiente','confirmada','cancelada'
}
