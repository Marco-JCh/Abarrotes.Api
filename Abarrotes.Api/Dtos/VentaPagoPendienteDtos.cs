namespace Abarrotes.Api.Dtos
{
    public class VentaPagoPendienteRequestDto
    {
        public int ClienteId { get; set; }
        public decimal Monto { get; set; }
    }

    public class VentaPagoPendienteResultDto
    {
        public int ClienteId { get; set; }

        public decimal MontoOriginal { get; set; }
        public decimal MontoUsado { get; set; }
        public decimal MontoSobrante { get; set; }

        public List<int> VentasPagadas { get; set; } = new();
        public List<int> VentasParcialmentePagadas { get; set; } = new();
    }
}
