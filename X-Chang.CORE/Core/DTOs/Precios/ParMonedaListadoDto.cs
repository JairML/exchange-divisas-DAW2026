namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class ParMonedaListadoDto
    {
        public string MonedaEntrega { get; set; } = string.Empty;
        public string MonedaObtiene { get; set; } = string.Empty;
        public decimal? MayorPrecioCompra { get; set; }
        public decimal? MenorPrecioVenta { get; set; }
        public decimal? Margen { get; set; }
    }
}
