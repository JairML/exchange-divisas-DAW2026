using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class GestionUsuariosAdminService : IGestionUsuariosAdminService
    {
        private readonly IGestionUsuariosAdminRepository _repository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public GestionUsuariosAdminService(
            IGestionUsuariosAdminRepository repository,
            ISesionUsuarioRepository sesionRepository)
        {
            _repository = repository;
            _sesionRepository = sesionRepository;
        }

        public async Task<List<UsuarioAdminResumenDto>> BuscarUsuariosAsync(
            string tokenSesion,
            FiltroUsuariosAdminDto filtro)
        {
            var administradorId = await ObtenerAdministradorIdAsync(tokenSesion);

            ValidarFiltro(filtro);

            return await _repository.BuscarUsuariosAsync(filtro);
        }

        public async Task<UsuarioAdminDetalleDto> ObtenerDetalleUsuarioAsync(
            string tokenSesion,
            int usuarioId)
        {
            var administradorId = await ObtenerAdministradorIdAsync(tokenSesion);

            var detalle = await _repository.ObtenerDetalleUsuarioAsync(usuarioId);

            if (detalle == null)
                throw new ArgumentException("El usuario no existe.");

            return detalle;
        }

        public async Task<CambiarEstadoUsuarioResponseDto> RestringirUsuarioAsync(
            string tokenSesion,
            int usuarioId,
            CambiarEstadoUsuarioRequestDto request)
        {
            var administradorId = await ObtenerAdministradorIdAsync(tokenSesion);

            ValidarMensaje(request.Mensaje);

            return await _repository.RestringirUsuarioAsync(
                administradorId,
                usuarioId,
                request.Mensaje.Trim());
        }

        public async Task<CambiarEstadoUsuarioResponseDto> HabilitarUsuarioAsync(
            string tokenSesion,
            int usuarioId,
            CambiarEstadoUsuarioRequestDto request)
        {
            var administradorId = await ObtenerAdministradorIdAsync(tokenSesion);

            ValidarMensaje(request.Mensaje);

            return await _repository.HabilitarUsuarioAsync(
                administradorId,
                usuarioId,
                request.Mensaje.Trim());
        }

        private async Task<int> ObtenerAdministradorIdAsync(string tokenSesion)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);

            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            var esAdmin = await _repository.EsAdministradorActivoAsync(sesion.UsuarioId);

            if (!esAdmin)
                throw new UnauthorizedAccessException("El usuario no tiene permisos de administrador.");

            return sesion.UsuarioId;
        }

        private static void ValidarFiltro(FiltroUsuariosAdminDto filtro)
        {
            if (!string.IsNullOrEmpty(filtro.NombreUsuario) &&
                filtro.NombreUsuario.Length > 30)
                throw new ArgumentException("Máximo 30 caracteres");

            if (!string.IsNullOrEmpty(filtro.CorreoElectronico) &&
                filtro.CorreoElectronico.Length > 100)
                throw new ArgumentException("Máximo 100 caracteres");

            var estado = filtro.Estado?.Trim() ?? "Todos";

            if (estado != "Todos" && estado != "Activo" && estado != "Restringido")
                throw new ArgumentException("Estado inválido.");
        }

        private static void ValidarMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("Ingrese un mensaje");

            if (mensaje.Length > 300)
                throw new ArgumentException("Máximo 300 caracteres");
        }
    }
}