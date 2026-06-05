using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Auth;

public record LoginRequestDto(
    [Required][StringLength(100, MinimumLength = 2)] string IdentificadorAcceso,
    [Required][StringLength(50, MinimumLength = 8)] string Password);
