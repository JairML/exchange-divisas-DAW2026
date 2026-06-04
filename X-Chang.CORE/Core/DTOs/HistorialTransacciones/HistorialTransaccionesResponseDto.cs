namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class HistorialTransaccionesResponseDto
    {
        public PaginadoDto<OrdenCompraHistorialDto> OrdenesCompra { get; set; } = new();
        public PaginadoDto<OfertaVentaHistorialDto> OfertasVenta { get; set; } = new();
        public PaginadoDto<CompraInmediataHistorialDto> ComprasInmediatas { get; set; } = new();
        public PaginadoDto<VentaInmediataHistorialDto> VentasInmediatas { get; set; } = new();
        public PaginadoDto<DepositoHistorialDto> Depositos { get; set; } = new();
        public PaginadoDto<RetiroHistorialDto> Retiros { get; set; } = new();
    }
}
