using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Auth;

public record RegisterRequestDto(
    [Required][StringLength(30, MinimumLength = 2)] string NombreUsuario,
    [Required][StringLength(100, MinimumLength = 5)][EmailAddress] string CorreoElectronico,
    [Required][StringLength(50, MinimumLength = 8)] string Password,
    [Required][StringLength(50, MinimumLength = 8)] string ConfirmarPassword,
    [Required][Range(1, int.MaxValue, ErrorMessage = "Seleccione un país")] int PaisId,
    string? Telefono = null,
    string? TipoDocumento = null,
    string? NumeroDocumento = null);
