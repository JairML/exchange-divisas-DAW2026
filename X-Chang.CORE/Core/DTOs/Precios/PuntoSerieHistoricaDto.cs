namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class PuntoSerieHistoricaDto
    {
        public DateTime FechaHora { get; set; }
        public decimal? MayorPrecioCompra { get; set; }
        public decimal? MenorPrecioVenta { get; set; }
        public decimal? Margen { get; set; }
    }
}
