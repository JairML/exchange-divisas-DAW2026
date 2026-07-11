using X_Chang.CORE.Core.DTOs.Mercado;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IMercadoService
    {
        Task<OperacionesActivasResponseDto> ObtenerOperacionesActivasAsync(string tokenSesion, FiltroOperacionesActivasDto filtro);
        Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId, bool verTodasOrdenes, bool verTodasOfertas);
        Task<ResumenOrdenCompraDto> ObtenerResumenOrdenCompraAsync(string tokenSesion, CrearOrdenCompraRequestDto request);
        Task<OrdenCompraResultadoDto> CrearOrdenCompraAsync(string tokenSesion, CrearOrdenCompraRequestDto request);
        Task<ResumenOfertaVentaDto> ObtenerResumenOfertaVentaAsync(string tokenSesion, CrearOfertaVentaRequestDto request);
        Task<OfertaVentaResultadoDto> CrearOfertaVentaAsync(string tokenSesion, CrearOfertaVentaRequestDto request);
        Task<PanelAdministrativoDto> ObtenerPanelAdministrativoAsync(string tokenSesion, FiltroPanelAdministrativoDto filtro);
        Task<ActividadRecientePaginadaDto> ObtenerActividadRecienteAsync(string tokenSesion, FiltroActividadRecienteDto filtro);
        Task<ExportarPanelAdminResponseDto> ExportarActividadRecienteExcelAsync(string tokenSesion, ExportarPanelAdminRequestDto filtro);
        Task<ExportarPanelAdminResponseDto> ExportarActividadRecientePdfAsync(string tokenSesion, ExportarPanelAdminRequestDto filtro);
    }
}
