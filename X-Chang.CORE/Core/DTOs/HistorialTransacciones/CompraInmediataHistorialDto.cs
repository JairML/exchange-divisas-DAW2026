namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class CompraInmediataHistorialDto
    {
        public int OperacionInmediataId { get; set; }
        public DateTime FechaHora { get; set; }
        public string ParMonedas { get; set; } = string.Empty;
        public decimal CantidadObtenida { get; set; }
        public decimal? PrecioMinCompra { get; set; }
        public decimal? PrecioMaxCompra { get; set; }
        public decimal? PrecioPromedioCompra { get; set; }
        public decimal TotalPagado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MetodoEjecucion { get; set; } = string.Empty;
        public bool TieneSaltos { get; set; }
        public List<CompraInmediataHistorialDto> SaltosRuta { get; set; } = new();
    }
}
