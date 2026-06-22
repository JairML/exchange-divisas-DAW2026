namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class ResumenOfertaVentaDto
    {
        public int ParMonedaId { get; set; }
        public string Par { get; set; } = string.Empty;
        public decimal CantidadAVender { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalEsperado { get; set; }
        public decimal SaldoDisponible { get; set; }
        public bool SaldoSuficiente { get; set; }
        public bool PuedeEjecutarseAutomaticamente { get; set; }
        public decimal CantidadEjecutableInmediata { get; set; }
        public decimal CantidadPendienteEstimada { get; set; }
        public decimal? PrecioMinimoVenta { get; set; }
        public decimal? PrecioMaximoVenta { get; set; }
        public decimal? PrecioPromedioVenta { get; set; }
        public decimal TotalEstimadoRecibido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
