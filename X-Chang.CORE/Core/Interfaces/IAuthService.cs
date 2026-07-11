using X_Chang.CORE.Core.DTOs.Auth;

namespace X_Chang.CORE.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task LogoutAsync(int usuarioId, string token);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task SolicitarRecuperacionAsync(string correo);
    Task RestablecerPasswordAsync(ResetPasswordRequestDto request);
}
