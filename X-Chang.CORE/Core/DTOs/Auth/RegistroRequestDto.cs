namespace X_Chang.CORE.Core.DTOs.Auth;

public class RegistroRequestDto
{
    public string NombreUsuario { get; set; } = null!;
    public string CorreoElectronico { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int PaisId { get; set; }
}
