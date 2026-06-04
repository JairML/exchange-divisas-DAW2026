using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IGestionUsuariosAdminRepository
    {
        Task<bool> EsAdministradorActivoAsync(int usuarioId);

        Task<List<UsuarioAdminResumenDto>> BuscarUsuariosAsync(FiltroUsuariosAdminDto filtro);

        Task<UsuarioAdminDetalleDto?> ObtenerDetalleUsuarioAsync(int usuarioId);

        Task<CambiarEstadoUsuarioResponseDto> RestringirUsuarioAsync(
            int administradorId,
            int usuarioId,
            string mensaje);

        Task<CambiarEstadoUsuarioResponseDto> HabilitarUsuarioAsync(
            int administradorId,
            int usuarioId,
            string mensaje);
    }
}