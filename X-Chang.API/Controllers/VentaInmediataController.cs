using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.CORE.Core.DTOs.VentaInmediata;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentaInmediataController : ControllerBase
    {
        private readonly IVentaInmediataService _service;
        private readonly INotificacionesCorreoService _notifService;

        public VentaInmediataController(
            IVentaInmediataService service,
            INotificacionesCorreoService notifService)
        {
            _service = service;
            _notifService = notifService;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

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

                await _notifService.EncolarAsync(
                    UsuarioId,
                    "VentaInmediata",
                    $"Venta inmediata ejecutada: {resultado.MonedaOrigen} → {resultado.MonedaDestino}",
                    $"Tu venta de {resultado.CantidadEjecutada} {resultado.MonedaOrigen} fue ejecutada exitosamente. " +
                    $"Total recibido: {resultado.TotalRecibido} {resultado.MonedaDestino}. " +
                    $"Estado: {resultado.Estado}. Fecha: {resultado.FechaOperacion:dd/MM/yyyy HH:mm}.",
                    "OperacionInmediata",
                    resultado.OperacionInmediataId);

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

                await _notifService.EncolarAsync(
                    UsuarioId,
                    "VentaInmediataMejorRuta",
                    $"Venta por mejor ruta ejecutada: {resultado.MonedaOrigen} → {resultado.MonedaDestino}",
                    $"Tu venta por mejor ruta de {resultado.CantidadEjecutada} {resultado.MonedaOrigen} fue ejecutada exitosamente. " +
                    $"Total recibido: {resultado.TotalRecibido} {resultado.MonedaDestino}. " +
                    $"Estado: {resultado.Estado}. Fecha: {resultado.FechaOperacion:dd/MM/yyyy HH:mm}.",
                    "OperacionInmediata",
                    resultado.OperacionInmediataId);

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
