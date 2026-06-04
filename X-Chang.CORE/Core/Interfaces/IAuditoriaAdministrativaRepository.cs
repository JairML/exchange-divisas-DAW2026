using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IAuditoriaAdministrativaRepository
    {
        Task<bool> EsAdministradorActivoAsync(int usuarioId);

        Task<AuditoriaAdminPaginadoDto> BuscarAuditoriaAsync(
            FiltroAuditoriaAdminDto filtro);

        Task<List<AuditoriaAdminRegistroDto>> BuscarAuditoriaParaExportarAsync(
            ExportarAuditoriaRequestDto filtro);
    }
}