using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Usuarios;

namespace X_Chang.CORE.Core.Interfaces;

public interface IUsuarioService
{
    Task<ResultadoOperacion<PerfilUsuarioDto>> ObtenerPerfilAsync(string tokenSesion);
    Task<ResultadoOperacion<bool>> CambiarPasswordAsync(string tokenSesion, CambiarPasswordRequestDto dto);
}
