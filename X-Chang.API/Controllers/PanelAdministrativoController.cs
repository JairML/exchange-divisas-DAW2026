using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PanelAdministrativoController : ControllerBase
    {
        private readonly IMercadoService _service;

        public PanelAdministrativoController(IMercadoService service)
        {
            _service = service;
        }

        private string ObtenerTokenSesion()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader["Bearer ".Length..].Trim();
            }

            if (Request.Headers.TryGetValue("tokenSesion", out var tokenSesion))
                return tokenSesion.ToString();

            throw new UnauthorizedAccessException("No se envió el token de sesión.");
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen([FromQuery] FiltroPanelAdministrativoDto filtro)
        {
            try
            {
                var resultado = await _service.ObtenerPanelAdministrativoAsync(ObtenerTokenSesion(), filtro);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message, detalle = ex.InnerException?.Message });
            }
        }
    }
}
