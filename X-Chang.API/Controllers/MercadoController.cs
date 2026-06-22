using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MercadoController : ControllerBase
    {
        private readonly IMercadoService _service;

        public MercadoController(IMercadoService service)
        {
            _service = service;
        }

        private string ObtenerTokenSesion()
        {
            if (!Request.Headers.TryGetValue("tokenSesion", out var token))
                throw new UnauthorizedAccessException("No se envió el token de sesión.");

            return token.ToString();
        }

        [HttpGet("operaciones-activas")]
        public async Task<IActionResult> ObtenerOperacionesActivas([FromQuery] FiltroOperacionesActivasDto filtro)
        {
            try
            {
                var resultado = await _service.ObtenerOperacionesActivasAsync(ObtenerTokenSesion(), filtro);
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

        [HttpGet("libro/{parMonedaId}")]
        public async Task<IActionResult> ObtenerLibro(
            int parMonedaId,
            [FromQuery] bool verTodasOrdenes = false,
            [FromQuery] bool verTodasOfertas = false)
        {
            try
            {
                var resultado = await _service.ObtenerLibroOrdenesAsync(parMonedaId, verTodasOrdenes, verTodasOfertas);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message, detalle = ex.InnerException?.Message });
            }
        }

        [HttpPost("ordenes/resumen")]
        public async Task<IActionResult> ObtenerResumenOrden([FromBody] CrearOrdenCompraRequestDto request)
        {
            try
            {
                var resultado = await _service.ObtenerResumenOrdenCompraAsync(ObtenerTokenSesion(), request);
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

        [HttpPost("ordenes")]
        public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenCompraRequestDto request)
        {
            try
            {
                var resultado = await _service.CrearOrdenCompraAsync(ObtenerTokenSesion(), request);
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

        [HttpPost("ofertas/resumen")]
        public async Task<IActionResult> ObtenerResumenOferta([FromBody] CrearOfertaVentaRequestDto request)
        {
            try
            {
                var resultado = await _service.ObtenerResumenOfertaVentaAsync(ObtenerTokenSesion(), request);
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

        [HttpPost("ofertas")]
        public async Task<IActionResult> CrearOferta([FromBody] CrearOfertaVentaRequestDto request)
        {
            try
            {
                var resultado = await _service.CrearOfertaVentaAsync(ObtenerTokenSesion(), request);
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
