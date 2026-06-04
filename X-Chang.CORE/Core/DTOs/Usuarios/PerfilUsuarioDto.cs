namespace X_Chang.CORE.Core.DTOs.Usuarios;

public class PerfilUsuarioDto
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string CorreoElectronico { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public string Pais { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public string TemaVisual { get; set; } = null!;
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
}
