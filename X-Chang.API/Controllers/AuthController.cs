using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.Auth;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegistrarAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        await _authService.LogoutAsync(usuarioId.Value, request.Token);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.CorreoElectronico))
            await _authService.SolicitarRecuperacionAsync(request.CorreoElectronico);

        return Ok(new { mensaje = "Si el correo está registrado, te enviamos un enlace para restablecer tu contraseña." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            await _authService.RestablecerPasswordAsync(request);
            return Ok(new { mensaje = "Tu contraseña fue actualizada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        return Ok(new
        {
            UsuarioId = usuarioId.Value,
            NombreUsuario = User.FindFirst("NombreUsuario")?.Value,
            CorreoElectronico = User.FindFirst("CorreoElectronico")?.Value,
            Rol = User.FindFirst("Rol")?.Value,
            TemaVisual = User.FindFirst("TemaVisual")?.Value
        });
    }

    [Authorize]
    [HttpGet("menu")]
    public IActionResult GetMenu()
    {
        var rol = User.FindFirst("Rol")?.Value;
        var opciones = new List<string>
        {
            "Menu principal", "Monedas", "Transacciones",
            "Historial", "Usuario", "Configuracion", "Cerrar sesion"
        };
        if (rol == "Administrador")
            opciones.Insert(opciones.Count - 1, "Administracion");

        return Ok(new { Rol = rol, Opciones = opciones });
    }
}
