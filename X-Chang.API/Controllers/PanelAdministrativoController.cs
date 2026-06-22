using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PanelAdministrativoController : ControllerBase
    {
        private readonly IMercadoService _service;

        public PanelAdministrativoController(IMercadoService service)
        {
            _service = service;
        }

        private string ObtenerTokenSesion()
        {
            if (!Request.Headers.TryGetValue("tokenSesion", out var token))
                throw new UnauthorizedAccessException("No se envió el token de sesión.");

            return token.ToString();
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
