using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Perfil;

public class ActualizarPerfilRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 30 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(500, ErrorMessage = "La URL de foto no puede superar los 500 caracteres.")]
    public string? FotoUrl { get; set; }
}
