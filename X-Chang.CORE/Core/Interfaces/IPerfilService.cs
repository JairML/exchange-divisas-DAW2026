using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IPerfilService
    {
        Task<PerfilDto> ObtenerPerfilAsync(int usuarioId);
        Task<PerfilDto> ActualizarPerfilAsync(int usuarioId, ActualizarPerfilRequestDto request);
    }
}
