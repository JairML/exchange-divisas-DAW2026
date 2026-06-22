namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class OfertaVentaResultadoDto
    {
        public int OfertaVentaId { get; set; }
        public int? OrdenCompraEspejoId { get; set; }
        public int ParMonedaId { get; set; }
        public string Par { get; set; } = string.Empty;
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadVendida { get; set; }
        public decimal CantidadPendiente { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalEsperado { get; set; }
        public decimal TotalRecibido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<EjecucionMercadoDto> Ejecuciones { get; set; } = new();
    }
}
