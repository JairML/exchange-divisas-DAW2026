using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Auth;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // POST api/auth/registro
    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroRequestDto dto)
    {
        try
        {
            var resultado = await _authService.RegistrarAsync(dto);

            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(resultado.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var resultado = await _authService.LoginAsync(dto);

            if (!resultado.Exito)
                return Unauthorized(new { mensaje = resultado.Mensaje });

            return Ok(resultado.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion)
    {
        try
        {
            var resultado = await _authService.LogoutAsync(tokenSesion);

            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = "Sesión cerrada correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
