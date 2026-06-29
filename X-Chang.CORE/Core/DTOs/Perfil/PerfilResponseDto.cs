namespace X_Chang.CORE.Core.DTOs.Perfil;

public class PerfilResponseDto
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public decimal CalificacionPromedio { get; set; }
    public int TotalTransaccionesCompletadas { get; set; }
}
