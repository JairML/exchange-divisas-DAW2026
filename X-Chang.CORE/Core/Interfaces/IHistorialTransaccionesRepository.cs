using X_Chang.CORE.Core.DTOs.HistorialTransacciones;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IHistorialTransaccionesRepository
    {
        Task<PaginadoDto<OrdenCompraHistorialDto>> ObtenerOrdenesCompraAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina);

        Task<PaginadoDto<OfertaVentaHistorialDto>> ObtenerOfertasVentaAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina);

        Task<PaginadoDto<CompraInmediataHistorialDto>> ObtenerComprasInmediatasAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina);

        Task<PaginadoDto<VentaInmediataHistorialDto>> ObtenerVentasInmediatasAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina);

        Task<PaginadoDto<DepositoHistorialDto>> ObtenerDepositosAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina);

        Task<PaginadoDto<RetiroHistorialDto>> ObtenerRetirosAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina);

        Task<HistorialTransaccionesResponseDto> ObtenerHistorialCompletoAsync(int usuarioId);
    }
}
