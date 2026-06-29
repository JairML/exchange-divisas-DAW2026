using X_Chang.CORE.Core.DTOs.Perfil;

namespace X_Chang.CORE.Core.Interfaces;

public interface IPerfilService
{
    Task<PerfilResponseDto> ObtenerPerfilAsync(int usuarioId);
    Task<PerfilResponseDto> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilRequestDto request);
}
