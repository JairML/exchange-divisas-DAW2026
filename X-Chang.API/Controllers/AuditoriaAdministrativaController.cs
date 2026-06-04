using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaAdministrativaController : ControllerBase
    {
        private readonly IAuditoriaAdministrativaService _service;

        public AuditoriaAdministrativaController(
            IAuditoriaAdministrativaService service)
        {
            _service = service;
        }

        private string ObtenerTokenSesion()
        {
            if (!Request.Headers.TryGetValue("tokenSesion", out var token))
                throw new UnauthorizedAccessException("No se envió el token de sesión.");

            return token.ToString();
        }

        [HttpGet("registros")]
        public async Task<IActionResult> BuscarAuditoria(
            [FromQuery] FiltroAuditoriaAdminDto filtro)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado = await _service.BuscarAuditoriaAsync(
                    token,
                    filtro);

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("exportar-excel")]
        public async Task<IActionResult> ExportarExcel(
            [FromBody] ExportarAuditoriaRequestDto filtro)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado = await _service.ExportarExcelAsync(
                    token,
                    filtro);

                return File(
                    resultado.Archivo,
                    resultado.TipoContenido,
                    resultado.NombreArchivo);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("exportar-pdf")]
        public async Task<IActionResult> ExportarPdf(
            [FromBody] ExportarAuditoriaRequestDto filtro)
        {
            try
            {
                var token = ObtenerTokenSesion();

                var resultado = await _service.ExportarPdfAsync(
                    token,
                    filtro);

                return File(
                    resultado.Archivo,
                    resultado.TipoContenido,
                    resultado.NombreArchivo);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }
    }
}