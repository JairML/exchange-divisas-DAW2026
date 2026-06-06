using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/configuracion")]
    [Authorize]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguracionUsuarioService _configuracionUsuarioService;

        public ConfiguracionController(IConfiguracionUsuarioService configuracionUsuarioService)
        {
            _configuracionUsuarioService = configuracionUsuarioService;
        }

        private string ObtenerTokenSesion()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("No se envió el token de sesión.");
            return authHeader["Bearer ".Length..].Trim();
        }

        [HttpGet("tema-visual")]
        public async Task<IActionResult> ObtenerTemaVisual()
        {
            try
            {
                var token = ObtenerTokenSesion();
                var result = await _configuracionUsuarioService.ObtenerTemaVisualAsync(token);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
        }

        [HttpPut("tema-visual")]
        public async Task<IActionResult> ActualizarTemaVisual([FromBody] ActualizarTemaVisualRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var result = await _configuracionUsuarioService.ActualizarTemaVisualAsync(token, request);
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
