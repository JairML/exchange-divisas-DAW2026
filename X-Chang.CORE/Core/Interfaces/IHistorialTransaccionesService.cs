using X_Chang.CORE.Core.DTOs.HistorialTransacciones;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IHistorialTransaccionesService
    {
        Task<HistorialTransaccionesResponseDto> ObtenerHistorialAsync(
            string tokenSesion, HistorialTransaccionesRequestDto request);

        Task<HistorialTransaccionesResponseDto> ObtenerParaExportarAsync(
            string tokenSesion, DateTime? fechaDesde, DateTime? fechaHasta, string? columna);

        Task<ExportarHistorialResponseDto> ExportarExcelAsync(
            string tokenSesion, ExportarHistorialRequestDto filtro);

        Task<ExportarHistorialResponseDto> ExportarPdfAsync(
            string tokenSesion, ExportarHistorialRequestDto filtro);
    }
}
