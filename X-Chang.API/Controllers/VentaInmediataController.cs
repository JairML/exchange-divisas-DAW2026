using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.VentaInmediata;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentaInmediataController : ControllerBase
    {
        private readonly IVentaInmediataService _service;

        public VentaInmediataController(IVentaInmediataService service)
        {
            _service = service;
        }

        private string ObtenerTokenSesion()
        {
            if (!Request.Headers.TryGetValue("tokenSesion", out var token))
                throw new UnauthorizedAccessException("No se envió el token de sesión.");

            return token.ToString();
        }

        [HttpPost("resumen")]
        public async Task<IActionResult> ObtenerResumen([FromBody] VentaInmediataRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ObtenerResumenVentaNormalAsync(token, request);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("confirmar")]
        public async Task<IActionResult> ConfirmarVenta([FromBody] ConfirmarVentaInmediataRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ConfirmarVentaNormalAsync(token, request);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("tiempo-busqueda/{saltos}")]
        public async Task<IActionResult> ObtenerTiempoBusqueda(int saltos)
        {
            try
            {
                var resultado = await _service.ObtenerTiempoEstimadoBusquedaRutaAsync(saltos);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("buscar-ruta")]
        public async Task<IActionResult> BuscarRuta([FromBody] BuscarMejorRutaVentaRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.BuscarMejorRutaAsync(token, request);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("cancelar-ruta/{busquedaRutaId}")]
        public async Task<IActionResult> CancelarBusquedaRuta(int busquedaRutaId)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.CancelarBusquedaRutaAsync(token, busquedaRutaId);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("confirmar-ruta")]
        public async Task<IActionResult> ConfirmarRuta([FromBody] ConfirmarVentaRutaRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ConfirmarVentaPorRutaAsync(token, request);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}