namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class EjecucionMercadoDto
    {
        public int EjecucionId { get; set; }
        public int OrdenCompraId { get; set; }
        public int OfertaVentaId { get; set; }
        public int CompradorId { get; set; }
        public int VendedorId { get; set; }
        public decimal CantidadEjecutada { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalOperacion { get; set; }
        public DateTime FechaEjecucion { get; set; }
    }
}
