using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Auth;

namespace X_Chang.CORE.Core.Interfaces;

public interface IAuthService
{
    Task<ResultadoOperacion<AuthResponseDto>> RegistrarAsync(RegistroRequestDto dto);
    Task<ResultadoOperacion<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ResultadoOperacion<bool>> LogoutAsync(string tokenSesion);
}
