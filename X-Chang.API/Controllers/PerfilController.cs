using Microsoft.AspNetCore.Mvc;
using X_Chang.CORE.Core.DTOs.Perfil;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/perfil")]
public class PerfilController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISesionUsuarioRepository _sesionRepo;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(
        IUsuarioRepository usuarioRepository,
        ISesionUsuarioRepository sesionRepo,
        ILogger<PerfilController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _sesionRepo = sesionRepo;
        _logger = logger;
    }

    private async Task<int?> ObtenerUsuarioIdAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var sessionToken = authHeader["Bearer ".Length..].Trim();
            var sesion = await _sesionRepo.ObtenerSesionActivaAsync(sessionToken);
            if (sesion != null)
                return sesion.UsuarioId;

            _logger.LogWarning("Bearer token recibido pero no coincide con una sesión activa.");
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var usuarioId = await ObtenerUsuarioIdAsync();
        if (usuarioId == null)
            return Unauthorized(new { mensaje = "Sesión inválida o expirada." });

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId.Value);
        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado." });

        return Ok(new PerfilResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            PaisResidencia = usuario.Pais?.Nombre,
            Rol = usuario.Rol?.Nombre,
            TemaVisual = usuario.TemaVisual,
            Estado = usuario.Estado,
            FechaRegistro = usuario.FechaRegistro,
        });
    }
}
