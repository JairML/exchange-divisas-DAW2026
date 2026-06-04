using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.CompraInmediata;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompraInmediataController : ControllerBase
    {
        private readonly ICompraInmediataService _service;

        public CompraInmediataController(ICompraInmediataService service)
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
        public async Task<IActionResult> ObtenerResumen(
            [FromBody] CompraInmediataRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado =
                    await _service.ObtenerResumenCompraNormalAsync(
                        token,
                        request);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("confirmar")]
        public async Task<IActionResult> ConfirmarCompra(
            [FromBody] ConfirmarCompraInmediataRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado =
                    await _service.ConfirmarCompraNormalAsync(
                        token,
                        request);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tiempo-busqueda/{saltos}")]
        public async Task<IActionResult> ObtenerTiempoBusqueda(
            int saltos)
        {
            try
            {
                var resultado =
                    await _service.ObtenerTiempoEstimadoBusquedaRutaAsync(
                        saltos);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("buscar-ruta")]
        public async Task<IActionResult> BuscarRuta(
            [FromBody] BuscarMejorRutaCompraRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado =
                    await _service.BuscarMejorRutaAsync(
                        token,
                        request);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("cancelar-ruta/{busquedaRutaId}")]
        public async Task<IActionResult> CancelarBusquedaRuta(
            int busquedaRutaId)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado =
                    await _service.CancelarBusquedaRutaAsync(
                        token,
                        busquedaRutaId);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("confirmar-ruta")]
        public async Task<IActionResult> ConfirmarRuta(
            [FromBody] ConfirmarCompraRutaRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado =
                    await _service.ConfirmarCompraPorRutaAsync(
                        token,
                        request);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}