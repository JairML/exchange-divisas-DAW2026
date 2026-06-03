using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/configuracion")]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguracionUsuarioService _configuracionUsuarioService;

        public ConfiguracionController(IConfiguracionUsuarioService configuracionUsuarioService)
        {
            _configuracionUsuarioService = configuracionUsuarioService;
        }

        [HttpGet("tema-visual")]
        public async Task<IActionResult> ObtenerTemaVisual(
            [FromHeader(Name = "X-Session-Token")] string tokenSesion)
        {
            try
            {
                var result = await _configuracionUsuarioService.ObtenerTemaVisualAsync(tokenSesion);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
        }

        [HttpPut("tema-visual")]
        public async Task<IActionResult> ActualizarTemaVisual(
            [FromHeader(Name = "X-Session-Token")] string tokenSesion,
            [FromBody] ActualizarTemaVisualRequestDto request)
        {
            try
            {
                var result = await _configuracionUsuarioService.ActualizarTemaVisualAsync(tokenSesion, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
        }
    }
}