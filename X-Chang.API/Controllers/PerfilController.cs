using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X_Chang.API.Helpers;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers
{
    [ApiController]
    [Route("api/perfil")]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _service;

        public PerfilController(IPerfilService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            try
            {
                return Ok(await _service.ObtenerPerfilAsync(usuarioId.Value));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilRequestDto request)
        {
            var usuarioId = this.GetUsuarioId();
            if (usuarioId == null) return Unauthorized();

            try
            {
                return Ok(await _service.ActualizarPerfilAsync(usuarioId.Value, request));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
