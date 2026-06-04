namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class GraficoPreciosParDto
    {
        public string MonedaOrigen { get; set; } = string.Empty;
        public string MonedaDestino { get; set; } = string.Empty;
        public List<PuntoSerieHistoricaDto> Serie { get; set; } = new();
    }
}
