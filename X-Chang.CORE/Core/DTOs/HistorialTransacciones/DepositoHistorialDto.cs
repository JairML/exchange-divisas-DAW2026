namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class DepositoHistorialDto
    {
        public int DepositoId { get; set; }
        public DateTime FechaHora { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public decimal MontoDepositado { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal ComisionAplicada { get; set; }
        public decimal TotalPagado { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
