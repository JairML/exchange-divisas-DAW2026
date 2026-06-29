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

    // Resuelve el UsuarioId a partir del token de sesión enviado como "Authorization: Bearer {token}".
    // Fallback: claim "UsuarioId" del JWT (cuando se integre) y header X-Usuario-Id (para pruebas).
    private async Task<int?> ObtenerUsuarioIdAsync()
    {
        // 1. Session token en Authorization: Bearer (mecanismo real del sistema)
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var sessionToken = authHeader["Bearer ".Length..].Trim();
            var sesion = await _sesionRepo.ObtenerSesionActivaAsync(sessionToken);
            if (sesion != null)
            {
                _logger.LogDebug("Sesión activa encontrada para usuario {UsuarioId}.", sesion.UsuarioId);
                return sesion.UsuarioId;
            }
            _logger.LogWarning("Bearer token recibido pero no coincide con ninguna sesión activa.");
        }

        // 2. Claim JWT "UsuarioId" (para integración futura con JWT)
        var claim = User?.FindFirst("UsuarioId")?.Value;
        if (int.TryParse(claim, out var idDesdeClaim))
            return idDesdeClaim;

        // 3. Header X-Usuario-Id (fallback de pruebas con Swagger/Postman)
        if (Request.Headers.TryGetValue("X-Usuario-Id", out var header))
            if (int.TryParse(header.FirstOrDefault(), out var idDesdeHeader))
                return idDesdeHeader;

        return null;
    }

    // GET api/perfil
    [HttpGet]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var usuarioId = await ObtenerUsuarioIdAsync();
        if (usuarioId == null)
        {
            _logger.LogWarning("GET /api/perfil: no se pudo resolver el UsuarioId (sin sesión activa, sin claim JWT, sin header X-Usuario-Id).");
            return Unauthorized(new { mensaje = "Sesión inválida o expirada." });
        }

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId.Value);
        if (usuario == null)
        {
            _logger.LogWarning("GET /api/perfil: usuario {UsuarioId} no encontrado en BD.", usuarioId.Value);
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        return Ok(new PerfilResponseDto
        {
            UsuarioId         = usuario.UsuarioId,
            NombreUsuario     = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            Telefono          = usuario.Telefono,
            FotoUrl           = usuario.FotoPerfilUrl,
            TemaVisual        = usuario.TemaVisual,
            Estado            = usuario.Estado,
            FechaRegistro     = usuario.FechaRegistro,
        });
    }

    // PUT api/perfil
    [HttpPut]
    public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilRequestDto dto)
    {
        var usuarioId = await ObtenerUsuarioIdAsync();
        if (usuarioId == null)
        {
            _logger.LogWarning("PUT /api/perfil: no se pudo resolver el UsuarioId.");
            return Unauthorized(new { mensaje = "Sesión inválida o expirada." });
        }

        _logger.LogInformation(
            "PUT /api/perfil: actualizando usuario {UsuarioId} | NombreUsuario={NombreUsuario} Telefono={Telefono} FotoUrl={FotoUrl}",
            usuarioId.Value, dto.NombreUsuario, dto.Telefono, dto.FotoUrl);

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId.Value);
        if (usuario == null)
        {
            _logger.LogWarning("PUT /api/perfil: usuario {UsuarioId} no encontrado.", usuarioId.Value);
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        if (usuario.Estado == "Restringido")
        {
            _logger.LogWarning("PUT /api/perfil: usuario {UsuarioId} está restringido.", usuarioId.Value);
            return BadRequest(new { mensaje = "Su cuenta se encuentra restringida y no puede modificar el perfil." });
        }

        try
        {
            usuario.NombreUsuario = dto.NombreUsuario.Trim();
            usuario.Telefono      = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            usuario.FotoPerfilUrl = string.IsNullOrWhiteSpace(dto.FotoUrl)  ? null : dto.FotoUrl.Trim();

            await _usuarioRepository.ActualizarAsync(usuario);

            _logger.LogInformation("PUT /api/perfil: usuario {UsuarioId} actualizado correctamente.", usuarioId.Value);

            return Ok(new PerfilResponseDto
            {
                UsuarioId         = usuario.UsuarioId,
                NombreUsuario     = usuario.NombreUsuario,
                CorreoElectronico = usuario.CorreoElectronico,
                Telefono          = usuario.Telefono,
                FotoUrl           = usuario.FotoPerfilUrl,
                TemaVisual        = usuario.TemaVisual,
                Estado            = usuario.Estado,
                FechaRegistro     = usuario.FechaRegistro,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT /api/perfil: error al actualizar usuario {UsuarioId}.", usuarioId.Value);

            // Construye la cadena completa: mensaje + InnerExceptions (útil para errores SQL de EF Core).
            var detalleCompleto = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                detalleCompleto += $" | Inner: {inner.Message}";
                inner = inner.InnerException;
            }

            return StatusCode(500, new { mensaje = "No se pudo actualizar el perfil.", detalle = detalleCompleto });
        }
    }
}
