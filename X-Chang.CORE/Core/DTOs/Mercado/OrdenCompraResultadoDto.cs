namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class OrdenCompraResultadoDto
    {
        public int OrdenCompraId { get; set; }
        public int? OfertaEspejoId { get; set; }
        public int ParMonedaId { get; set; }
        public string Par { get; set; } = string.Empty;
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadObtenida { get; set; }
        public decimal CantidadPendiente { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalComprometido { get; set; }
        public decimal TotalEjecutado { get; set; }
        public decimal MontoReembolsadoPorMejorPrecio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<EjecucionMercadoDto> Ejecuciones { get; set; } = new();
    }
}
