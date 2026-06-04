namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class OfertaVentaHistorialDto
    {
        public int OfertaVentaId { get; set; }
        public DateTime FechaHora { get; set; }
        public string ParMonedas { get; set; } = string.Empty;
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadVendida { get; set; }
        public decimal CantidadPendiente { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalEsperado { get; set; }
        public decimal TotalRecibido { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
