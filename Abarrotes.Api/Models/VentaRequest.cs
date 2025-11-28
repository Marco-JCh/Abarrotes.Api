namespace Abarrotes.Api.Models
{
    /// <summary>
    /// Registro mínimo para idempotencia de ventas.
    /// Cada POST de venta debe enviar un RequestKey (GUID).
    /// Si el mismo RequestKey llega dos veces, la segunda se rechaza por índice único.
    /// </summary>
    public class VentaRequest
    {
        public int Id { get; set; }

        /// <summary>
        /// Clave idempotente única por solicitud (GUID en texto).
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Marca de tiempo para auditoría.
        /// </summary>
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
