namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class LibroOrdenesDto
    {
        public int ParMonedaId { get; set; }
        public string Par { get; set; } = string.Empty;
        public List<LibroOrdenesRegistroDto> OrdenesCompra { get; set; } = new();
        public List<LibroOrdenesRegistroDto> OfertasVenta { get; set; } = new();
        public string MensajeOrdenes { get; set; } = string.Empty;
        public string MensajeOfertas { get; set; } = string.Empty;
    }
}
