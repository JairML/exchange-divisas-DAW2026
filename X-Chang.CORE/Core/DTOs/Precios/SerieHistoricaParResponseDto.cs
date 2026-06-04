namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class SerieHistoricaParResponseDto
    {
        public string MonedaOrigen { get; set; } = string.Empty;
        public string MonedaDestino { get; set; } = string.Empty;
        public string Rango { get; set; } = string.Empty;

        // Precios actuales calculados desde el order book vivo
        public decimal? MayorPrecioCompraActual { get; set; }
        public decimal? MenorPrecioVentaActual { get; set; }
        public decimal? MargenActual { get; set; }

        public List<PuntoSerieHistoricaDto> Serie { get; set; } = new();
    }
}
