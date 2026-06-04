using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class ConfiguracionUsuarioService : IConfiguracionUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISesionUsuarioRepository _sesionUsuarioRepository;

        public ConfiguracionUsuarioService(
            IUsuarioRepository usuarioRepository,
            ISesionUsuarioRepository sesionUsuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
            _sesionUsuarioRepository = sesionUsuarioRepository;
        }

        public async Task<TemaVisualResponseDto> ObtenerTemaVisualAsync(string tokenSesion)
        {
            var usuario = await ObtenerUsuarioDesdeSesionAsync(tokenSesion);

            return new TemaVisualResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                TemaVisual = usuario.TemaVisual
            };
        }

        public async Task<TemaVisualResponseDto> ActualizarTemaVisualAsync(
            string tokenSesion,
            ActualizarTemaVisualRequestDto request)
        {
            if (request.TemaVisual != "Claro" && request.TemaVisual != "Oscuro")
            {
                throw new ArgumentException("El tema visual debe ser Claro u Oscuro.");
            }

            var usuario = await ObtenerUsuarioDesdeSesionAsync(tokenSesion);

            usuario.TemaVisual = request.TemaVisual;

            await _usuarioRepository.ActualizarAsync(usuario);

            return new TemaVisualResponseDto
            {
                UsuarioId = usuario.UsuarioId,
                NombreUsuario = usuario.NombreUsuario,
                TemaVisual = usuario.TemaVisual
            };
        }

        private async Task<X_Chang.CORE.Core.Entities.Usuarios> ObtenerUsuarioDesdeSesionAsync(string tokenSesion)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
            {
                throw new UnauthorizedAccessException("Sesión no enviada.");
            }

            var sesion = await _sesionUsuarioRepository.ObtenerSesionActivaAsync(tokenSesion);

            if (sesion == null)
            {
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");
            }

            var usuario = await _usuarioRepository.ObtenerPorIdAsync(sesion.UsuarioId);

            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Usuario no encontrado.");
            }

            if (usuario.Estado == "Restringido")
            {
                throw new UnauthorizedAccessException("Usuario restringido.");
            }

            return usuario;
        }
    }
}