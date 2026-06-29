using X_Chang.CORE.Core.DTOs.Perfil;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class PerfilService : IPerfilService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHistorialTransaccionesRepository _historialRepository;

    public PerfilService(
        IUsuarioRepository usuarioRepository,
        IHistorialTransaccionesRepository historialRepository)
    {
        _usuarioRepository = usuarioRepository;
        _historialRepository = historialRepository;
    }

    public async Task<PerfilResponseDto> ObtenerPerfilAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var totalTransacciones = await _historialRepository.ContarTransaccionesCompletadasAsync(usuarioId);

        return new PerfilResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            Telefono = usuario.Telefono,
            FotoUrl = usuario.FotoUrl,
            TipoDocumento = usuario.TipoDocumento,
            NumeroDocumento = usuario.NumeroDocumento,
            CalificacionPromedio = 0,
            TotalTransaccionesCompletadas = totalTransacciones
        };
    }

    public async Task<PerfilResponseDto> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilRequestDto request)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (request.NombreUsuario != null)
            usuario.NombreUsuario = request.NombreUsuario;

        if (request.Telefono != null)
            usuario.Telefono = request.Telefono;

        if (request.FotoUrl != null)
            usuario.FotoUrl = request.FotoUrl;

        await _usuarioRepository.ActualizarAsync(usuario);

        var totalTransacciones = await _historialRepository.ContarTransaccionesCompletadasAsync(usuarioId);

        return new PerfilResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            Telefono = usuario.Telefono,
            FotoUrl = usuario.FotoUrl,
            TipoDocumento = usuario.TipoDocumento,
            NumeroDocumento = usuario.NumeroDocumento,
            CalificacionPromedio = 0,
            TotalTransaccionesCompletadas = totalTransacciones
        };
    }
}
