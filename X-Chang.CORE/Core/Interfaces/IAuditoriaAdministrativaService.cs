using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IAuditoriaAdministrativaService
    {
        Task<AuditoriaAdminPaginadoDto> BuscarAuditoriaAsync(
            string tokenSesion,
            FiltroAuditoriaAdminDto filtro);

        Task<ExportarAuditoriaResponseDto> ExportarExcelAsync(
            string tokenSesion,
            ExportarAuditoriaRequestDto filtro);

        Task<ExportarAuditoriaResponseDto> ExportarPdfAsync(
            string tokenSesion,
            ExportarAuditoriaRequestDto filtro);
    }
}