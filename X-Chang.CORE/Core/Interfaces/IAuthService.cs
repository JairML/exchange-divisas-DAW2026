using X_Chang.CORE.Core.DTOs.Auth;

namespace X_Chang.CORE.Core.Interfaces;

public interface IAuthService
{
    // US-001
    Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto request);
}
