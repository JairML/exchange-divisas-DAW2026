using System.ComponentModel.DataAnnotations;

namespace X_Chang.CORE.Core.DTOs.Auth;

// US-001: datos que el usuario envía al registrarse.
public record RegisterRequestDto(
    [Required][StringLength(30, MinimumLength = 2)] string NombreUsuario,
    [Required][StringLength(100, MinimumLength = 5)][EmailAddress] string CorreoElectronico,
    [Required][StringLength(50, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$",
        ErrorMessage = "La contraseña debe contener una mayúscula, un número y un carácter especial")]
    string Password,
    [Required][StringLength(50, MinimumLength = 8)] string ConfirmarPassword,
    [Required][Range(1, int.MaxValue, ErrorMessage = "Seleccione un país")] int PaisId);
