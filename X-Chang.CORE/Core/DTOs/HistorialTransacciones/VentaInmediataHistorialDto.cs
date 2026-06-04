namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class VentaInmediataHistorialDto
    {
        public int OperacionInmediataId { get; set; }
        public DateTime FechaHora { get; set; }
        public string ParMonedas { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal? PrecioMinVenta { get; set; }
        public decimal? PrecioMaxVenta { get; set; }
        public decimal? PrecioPromedioVenta { get; set; }
        public decimal TotalRecibido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MetodoEjecucion { get; set; } = string.Empty;
        public bool TieneSaltos { get; set; }
        public List<VentaInmediataHistorialDto> SaltosRuta { get; set; } = new();
    }
}
