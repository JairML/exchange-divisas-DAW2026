using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Auth;

public record ForgotPasswordRequestDto([Required] string CorreoElectronico);
