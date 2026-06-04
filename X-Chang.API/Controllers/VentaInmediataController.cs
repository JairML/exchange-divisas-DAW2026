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
        private readonly ISesionUsuarioRepository _sesionRepo;
        private readonly INotificacionesCorreoService _notifService;

        public VentaInmediataController(
            IVentaInmediataService service,
            ISesionUsuarioRepository sesionRepo,
            INotificacionesCorreoService notifService)
        {
            _service = service;
            _sesionRepo = sesionRepo;
            _notifService = notifService;
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

                var sesion = await _sesionRepo.ObtenerSesionActivaAsync(token);
                if (sesion != null)
                {
                    await _notifService.EncolarAsync(
                        sesion.UsuarioId,
                        "VentaInmediata",
                        $"Venta inmediata ejecutada: {resultado.MonedaOrigen} → {resultado.MonedaDestino}",
                        $"Tu venta de {resultado.CantidadEjecutada} {resultado.MonedaOrigen} fue ejecutada exitosamente. " +
                        $"Total recibido: {resultado.TotalRecibido} {resultado.MonedaDestino}. " +
                        $"Estado: {resultado.Estado}. Fecha: {resultado.FechaOperacion:dd/MM/yyyy HH:mm}.",
                        "OperacionInmediata",
                        resultado.OperacionInmediataId);
                }

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

                var sesion = await _sesionRepo.ObtenerSesionActivaAsync(token);
                if (sesion != null)
                {
                    await _notifService.EncolarAsync(
                        sesion.UsuarioId,
                        "VentaInmediataMejorRuta",
                        $"Venta por mejor ruta ejecutada: {resultado.MonedaOrigen} → {resultado.MonedaDestino}",
                        $"Tu venta por mejor ruta de {resultado.CantidadEjecutada} {resultado.MonedaOrigen} fue ejecutada exitosamente. " +
                        $"Total recibido: {resultado.TotalRecibido} {resultado.MonedaDestino}. " +
                        $"Estado: {resultado.Estado}. Fecha: {resultado.FechaOperacion:dd/MM/yyyy HH:mm}.",
                        "OperacionInmediata",
                        resultado.OperacionInmediataId);
                }

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
