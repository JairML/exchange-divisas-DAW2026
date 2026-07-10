using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GestionUsuariosAdminController : ControllerBase
    {
        private readonly IGestionUsuariosAdminService _service;

        public GestionUsuariosAdminController(IGestionUsuariosAdminService service)
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

        [HttpGet("usuarios")]
        public async Task<IActionResult> BuscarUsuarios([FromQuery] FiltroUsuariosAdminDto filtro)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.BuscarUsuariosAsync(token, filtro);
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

        [HttpGet("usuarios/{usuarioId}")]
        public async Task<IActionResult> ObtenerDetalleUsuario(int usuarioId)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.ObtenerDetalleUsuarioAsync(token, usuarioId);
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

        [HttpPost("usuarios/{usuarioId}/restringir")]
        public async Task<IActionResult> RestringirUsuario(int usuarioId, [FromBody] CambiarEstadoUsuarioRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.RestringirUsuarioAsync(token, usuarioId, request);
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

        [HttpPost("usuarios/{usuarioId}/habilitar")]
        public async Task<IActionResult> HabilitarUsuario(int usuarioId, [FromBody] CambiarEstadoUsuarioRequestDto request)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.HabilitarUsuarioAsync(token, usuarioId, request);
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

        [HttpPost("usuarios/{usuarioId}/mensaje-ia")]
        public async Task<IActionResult> GenerarMensajeIa(
            int usuarioId,
            [FromBody] GenerarMensajeIaRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var token = ObtenerTokenSesion();
                var resultado = await _service.GenerarMensajeIaAsync(token, usuarioId, request, cancellationToken);
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
