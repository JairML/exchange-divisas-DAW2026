namespace X_Chang.CORE.Core.DTOs.Usuarios;

public class CambiarPasswordRequestDto
{
    public string PasswordActual { get; set; } = null!;
    public string PasswordNuevo { get; set; } = null!;
}
