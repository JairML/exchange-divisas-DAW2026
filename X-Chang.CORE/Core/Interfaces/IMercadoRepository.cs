using X_Chang.CORE.Core.DTOs.Mercado;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IMercadoRepository
    {
        Task<OperacionesActivasResponseDto> ObtenerOperacionesActivasAsync(int usuarioId, FiltroOperacionesActivasDto filtro);
        Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId, bool verTodasOrdenes, bool verTodasOfertas);
        Task<ResumenOrdenCompraDto> ObtenerResumenOrdenCompraAsync(int usuarioId, CrearOrdenCompraRequestDto request);
        Task<OrdenCompraResultadoDto> CrearOrdenCompraAsync(int usuarioId, CrearOrdenCompraRequestDto request);
        Task<ResumenOfertaVentaDto> ObtenerResumenOfertaVentaAsync(int usuarioId, CrearOfertaVentaRequestDto request);
        Task<OfertaVentaResultadoDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaVentaRequestDto request);
        Task<PanelAdministrativoDto> ObtenerPanelAdministrativoAsync(FiltroPanelAdministrativoDto filtro);
        Task<ActividadRecientePaginadaDto> ObtenerActividadRecienteAsync(FiltroActividadRecienteDto filtro);
        Task<List<ActividadRecienteAdminDto>> ObtenerActividadRecienteParaExportarAsync(DateTime? fechaDesde, DateTime? fechaHasta);
        Task<bool> EsAdministradorActivoAsync(int usuarioId);
    }
}
