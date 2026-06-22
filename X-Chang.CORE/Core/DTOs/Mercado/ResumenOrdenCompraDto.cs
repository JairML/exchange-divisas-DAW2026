namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class ResumenOrdenCompraDto
    {
        public int ParMonedaId { get; set; }
        public string Par { get; set; } = string.Empty;
        public decimal CantidadAObtener { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalComprometido { get; set; }
        public decimal SaldoDisponible { get; set; }
        public bool SaldoSuficiente { get; set; }
        public bool PuedeEjecutarseAutomaticamente { get; set; }
        public decimal CantidadEjecutableInmediata { get; set; }
        public decimal CantidadPendienteEstimada { get; set; }
        public decimal? PrecioMinimoCompra { get; set; }
        public decimal? PrecioMaximoCompra { get; set; }
        public decimal? PrecioPromedioCompra { get; set; }
        public decimal TotalEstimadoEjecutado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
