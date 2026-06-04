using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Usuarios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService) => _usuarioService = usuarioService;

    // GET api/usuarios/perfil
    [HttpGet("perfil")]
    public async Task<IActionResult> ObtenerPerfil(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion)
    {
        try
        {
            var resultado = await _usuarioService.ObtenerPerfilAsync(tokenSesion);

            if (!resultado.Exito)
                return Unauthorized(new { mensaje = resultado.Mensaje });

            return Ok(resultado.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // PUT api/usuarios/contrasena
    [HttpPut("contrasena")]
    public async Task<IActionResult> CambiarPassword(
        [FromHeader(Name = "X-Session-Token")] string tokenSesion,
        [FromBody] CambiarPasswordRequestDto dto)
    {
        try
        {
            var resultado = await _usuarioService.CambiarPasswordAsync(tokenSesion, dto);

            if (!resultado.Exito)
                return resultado.Mensaje!.Contains("Sesión")
                    ? Unauthorized(new { mensaje = resultado.Mensaje })
                    : BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
