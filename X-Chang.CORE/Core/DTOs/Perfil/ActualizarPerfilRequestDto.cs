using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Perfil;

public class ActualizarPerfilRequestDto
{
    [StringLength(30, MinimumLength = 2)]
    public string? NombreUsuario { get; set; }

    [StringLength(20)]
    public string? Telefono { get; set; }

    [StringLength(500)]
    public string? FotoUrl { get; set; }
}
