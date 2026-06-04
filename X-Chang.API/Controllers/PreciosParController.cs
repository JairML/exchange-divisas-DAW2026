using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.Precios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    // US-009, US-010, US-011: visualización de precios y pares de monedas.
    // Todos los endpoints aceptan un usuario autenticado opcional (X-Usuario-Id / claim JWT).
    // [Authorize] se habilitará cuando esté integrado el login (US-001/US-002).
    [Route("api/preciospares")]
    [ApiController]
    public class PreciosParController : ControllerBase
    {
        private readonly IPreciosParService _service;

        public PreciosParController(IPreciosParService service)
        {
            _service = service;
        }

        // ─── US-009 ──────────────────────────────────────────────────────────────
        // GET api/preciospares/menu-principal
        // Sin autenticar → serie histórica USD/EUR (1 gráfico).
        // Autenticado   → 2 gráficos basados en la moneda principal y la última orden activa.
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

        // ─── US-010 ──────────────────────────────────────────────────────────────
        // GET api/preciospares
        // Query params: monedaEntrega, monedaObtiene, criterio, direccion,
        //               colapsarParesInversos, pagina, registrosPorPagina
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

        // ─── US-011 ──────────────────────────────────────────────────────────────
        // GET api/preciospares/serie/{monedaOrigen}/{monedaDestino}?rango=UltimoDia
        // Devuelve indicadores actuales + serie histórica para el par indicado.
        // rango: UltimoDia (default) | UltimaSemana | UltimoMes | UltimoAno | Total
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
