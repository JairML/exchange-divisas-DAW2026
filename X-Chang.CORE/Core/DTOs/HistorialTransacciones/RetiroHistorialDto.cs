namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class RetiroHistorialDto
    {
        public int RetiroId { get; set; }
        public DateTime FechaHora { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public decimal MontoRetirado { get; set; }
        public string MetodoCobro { get; set; } = string.Empty;
        public decimal ComisionAplicada { get; set; }
        public decimal MontoFinalRecibido { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
