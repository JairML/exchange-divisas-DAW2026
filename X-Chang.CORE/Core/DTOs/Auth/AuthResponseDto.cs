namespace X_Chang.CORE.Core.DTOs.Auth;

// US-001 / US-002: respuesta devuelta tras registro o inicio de sesión.
public record AuthResponseDto(string Token, DateTime Expira, UsuarioInfoDto Usuario);

public record UsuarioInfoDto(
    int UsuarioId,
    string NombreUsuario,
    string CorreoElectronico,
    string Rol,
    string TemaVisual,
    string Estado);
