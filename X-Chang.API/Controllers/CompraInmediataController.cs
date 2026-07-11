using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs.CompraInmediata;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompraInmediataController : ControllerBase
    {
        private readonly ICompraInmediataService _service;
        private readonly INotificacionesCorreoService _notifService;

        public CompraInmediataController(
            ICompraInmediataService service,
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
        public async Task<IActionResult> ObtenerResumen([FromBody] CompraInmediataRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ObtenerResumenCompraNormalAsync(token, request);
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
        public async Task<IActionResult> ConfirmarCompra([FromBody] ConfirmarCompraInmediataRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ConfirmarCompraNormalAsync(token, request);

                await _notifService.EncolarAsync(
                    UsuarioId,
                    "CompraInmediata",
                    $"Compra inmediata ejecutada: {resultado.MonedaOrigen} → {resultado.MonedaDestino}",
                    EmailHtmlBuilder.Build(
                        "Compra inmediata ejecutada",
                        $"Tu compra de {resultado.MonedaOrigen} → {resultado.MonedaDestino} fue ejecutada exitosamente.",
                        [
                            ("Par de monedas",    $"{resultado.MonedaOrigen} → {resultado.MonedaDestino}"),
                            ("Cantidad obtenida", $"{resultado.CantidadEjecutada.ToString("N2")} {resultado.MonedaDestino}"),
                            ("Total pagado",      $"{resultado.TotalPagado.ToString("N2")} {resultado.MonedaOrigen}"),
                            ("Estado",            resultado.Estado),
                            ("Fecha y hora",      resultado.FechaOperacion.ToString("dd/MM/yyyy HH:mm")),
                        ]),
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
        public async Task<IActionResult> BuscarRuta([FromBody] BuscarMejorRutaCompraRequestDto request)
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
        public async Task<IActionResult> ConfirmarRuta([FromBody] ConfirmarCompraRutaRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ConfirmarCompraPorRutaAsync(token, request);

                await _notifService.EncolarAsync(
                    UsuarioId,
                    "CompraInmediataMejorRuta",
                    $"Compra por mejor ruta ejecutada: {resultado.MonedaOrigen} → {resultado.MonedaDestino}",
                    EmailHtmlBuilder.Build(
                        "Compra por mejor ruta ejecutada",
                        $"Tu compra por mejor ruta de {resultado.MonedaOrigen} → {resultado.MonedaDestino} fue ejecutada exitosamente.",
                        [
                            ("Par de monedas",    $"{resultado.MonedaOrigen} → {resultado.MonedaDestino}"),
                            ("Cantidad obtenida", $"{resultado.CantidadEjecutada.ToString("N2")} {resultado.MonedaDestino}"),
                            ("Total pagado",      $"{resultado.TotalPagado.ToString("N2")} {resultado.MonedaOrigen}"),
                            ("Estado",            resultado.Estado),
                            ("Fecha y hora",      resultado.FechaOperacion.ToString("dd/MM/yyyy HH:mm")),
                        ]),
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
