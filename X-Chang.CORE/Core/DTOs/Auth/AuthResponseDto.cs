namespace X_Chang.CORE.Core.DTOs.Auth;

public record AuthResponseDto(string Token, DateTime Expira, UsuarioInfoDto Usuario);

public record UsuarioInfoDto(
    int UsuarioId,
    string NombreUsuario,
    string CorreoElectronico,
    string Rol,
    string TemaVisual,
    string Estado);
