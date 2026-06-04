using BCrypt.Net;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Usuarios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ISesionUsuarioRepository _sesionRepo;

    public UsuarioService(IUsuarioRepository usuarioRepo, ISesionUsuarioRepository sesionRepo)
    {
        _usuarioRepo = usuarioRepo;
        _sesionRepo = sesionRepo;
    }

    public async Task<ResultadoOperacion<PerfilUsuarioDto>> ObtenerPerfilAsync(string tokenSesion)
    {
        var usuario = await ValidarSesionAsync(tokenSesion);
        if (usuario == null)
            return ResultadoOperacion<PerfilUsuarioDto>.Error("Sesión inválida o expirada.");

        var completo = await _usuarioRepo.ObtenerConRolYPaisAsync(usuario.UsuarioId);
        if (completo == null)
            return ResultadoOperacion<PerfilUsuarioDto>.Error("Usuario no encontrado.");

        return ResultadoOperacion<PerfilUsuarioDto>.Ok(new PerfilUsuarioDto
        {
            UsuarioId = completo.UsuarioId,
            NombreUsuario = completo.NombreUsuario,
            CorreoElectronico = completo.CorreoElectronico,
            Rol = completo.Rol.Nombre,
            Pais = completo.Pais.Nombre,
            Estado = completo.Estado,
            TemaVisual = completo.TemaVisual,
            FechaRegistro = completo.FechaRegistro,
            FechaUltimoAcceso = completo.FechaUltimoAcceso
        });
    }

    public async Task<ResultadoOperacion<bool>> CambiarPasswordAsync(
        string tokenSesion, CambiarPasswordRequestDto dto)
    {
        var usuario = await ValidarSesionAsync(tokenSesion);
        if (usuario == null)
            return ResultadoOperacion<bool>.Error("Sesión inválida o expirada.");

        if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.PasswordHash))
            return ResultadoOperacion<bool>.Error("La contraseña actual es incorrecta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNuevo);
        await _usuarioRepo.ActualizarAsync(usuario);

        return ResultadoOperacion<bool>.Ok(true);
    }

    private async Task<X_Chang.CORE.Core.Entities.Usuarios?> ValidarSesionAsync(string tokenSesion)
    {
        if (string.IsNullOrWhiteSpace(tokenSesion))
            return null;

        var sesion = await _sesionRepo.ObtenerSesionActivaAsync(tokenSesion);
        if (sesion == null)
            return null;

        var usuario = await _usuarioRepo.ObtenerPorIdAsync(sesion.UsuarioId);
        if (usuario == null || usuario.Estado != "Activo")
            return null;

        return usuario;
    }
}
