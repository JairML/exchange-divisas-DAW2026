namespace X_Chang.CORE.Core.DTOs.Auth;

public class LoginRequestDto
{
    /// <summary>Puede ser NombreUsuario o CorreoElectronico.</summary>
    public string Credencial { get; set; } = null!;
    public string Password { get; set; } = null!;
}
