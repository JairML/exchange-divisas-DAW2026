using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IGestionUsuariosAdminService
    {
        Task<List<UsuarioAdminResumenDto>> BuscarUsuariosAsync(
            string tokenSesion,
            FiltroUsuariosAdminDto filtro);

        Task<UsuarioAdminDetalleDto> ObtenerDetalleUsuarioAsync(
            string tokenSesion,
            int usuarioId);

        Task<CambiarEstadoUsuarioResponseDto> RestringirUsuarioAsync(
            string tokenSesion,
            int usuarioId,
            CambiarEstadoUsuarioRequestDto request);

        Task<CambiarEstadoUsuarioResponseDto> HabilitarUsuarioAsync(
            string tokenSesion,
            int usuarioId,
            CambiarEstadoUsuarioRequestDto request);
    }
}