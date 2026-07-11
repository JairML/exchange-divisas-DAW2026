using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Auth;

public record ResetPasswordRequestDto(
    [Required] string Token,
    [Required][StringLength(50, MinimumLength = 8)] string NuevaPassword,
    [Required][StringLength(50, MinimumLength = 8)] string ConfirmarPassword);
