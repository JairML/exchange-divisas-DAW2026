namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class CrearOrdenCompraRequestDto
    {
        public int ParMonedaId { get; set; }
        public decimal CantidadAObtener { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
