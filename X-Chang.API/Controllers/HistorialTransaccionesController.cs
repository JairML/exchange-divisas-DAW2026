using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.HistorialTransacciones;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/historial-transacciones")]
    [Authorize]
    public class HistorialTransaccionesController : ControllerBase
    {
        private readonly IHistorialTransaccionesService _service;

        public HistorialTransaccionesController(IHistorialTransaccionesService service)
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

        [HttpGet]
        public async Task<IActionResult> ObtenerHistorial([FromQuery] HistorialTransaccionesRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ObtenerHistorialAsync(token, request);
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

        [HttpGet("exportar")]
        public async Task<IActionResult> ExportarHistorial(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] string? columna)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ObtenerParaExportarAsync(token, fechaDesde, fechaHasta, columna);
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

        [HttpPost("exportar-excel")]
        public async Task<IActionResult> ExportarExcel([FromBody] ExportarHistorialRequestDto filtro)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ExportarExcelAsync(token, filtro);
                return File(resultado.Archivo, resultado.TipoContenido, resultado.NombreArchivo);
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

        [HttpPost("exportar-pdf")]
        public async Task<IActionResult> ExportarPdf([FromBody] ExportarHistorialRequestDto filtro)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ExportarPdfAsync(token, filtro);
                return File(resultado.Archivo, resultado.TipoContenido, resultado.NombreArchivo);
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
