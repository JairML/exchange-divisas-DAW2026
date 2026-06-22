using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.Precios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [Route("api/preciospares")]
    [ApiController]
    public class PreciosParController : ControllerBase
    {
        private readonly IPreciosParService _service;

        public PreciosParController(IPreciosParService service)
        {
            _service = service;
        }

        [HttpGet("menu-principal")]
        public async Task<IActionResult> ObtenerMenuPrincipal()
        {
            try
            {
                var usuarioId = this.GetUsuarioId();
                var resultado = await _service.ObtenerDatosMenuPrincipalAsync(usuarioId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerListadoPares([FromQuery] FiltroParesMonedaDto filtro)
        {
            try
            {
                var usuarioId = this.GetUsuarioId();
                var resultado = await _service.ObtenerListadoParesAsync(usuarioId, filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("serie/{monedaOrigen}/{monedaDestino}")]
        public async Task<IActionResult> ObtenerSerieHistorica(
            string monedaOrigen,
            string monedaDestino,
            [FromQuery] string rango = "UltimoDia")
        {
            try
            {
                var resultado = await _service.ObtenerSerieHistoricaAsync(monedaOrigen, monedaDestino, rango);

                if (!resultado.Exito)
                    return BadRequest(new { mensaje = resultado.Mensaje });

                return Ok(resultado.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
