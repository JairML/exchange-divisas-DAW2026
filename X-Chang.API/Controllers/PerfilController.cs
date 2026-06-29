using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.Perfil;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _perfilService;

    public PerfilController(IPerfilService perfilService)
    {
        _perfilService = perfilService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var perfil = await _perfilService.ObtenerPerfilAsync(usuarioId.Value);
            return Ok(perfil);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilRequestDto request)
    {
        var usuarioId = this.GetUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var perfil = await _perfilService.ActualizarPerfilAsync(usuarioId.Value, request);
            return Ok(perfil);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
