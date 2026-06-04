using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.API.Authentication;

// US-002: valida el token de sesión opaco enviado como Bearer en cada petición.
public class SessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ExchangeDivisasDbContext _context;

    public SessionAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ExchangeDivisasDbContext context)
        : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.NoResult();

        var sesion = await _context.SesionesUsuario
            .Include(s => s.Usuario).ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(s => s.TokenSesion == token && s.Estado == "Activa");

        if (sesion == null || sesion.FechaExpiracion < DateTime.UtcNow)
            return AuthenticateResult.Fail("Token de sesión inválido o expirado.");

        if (sesion.Usuario.Estado != "Activo")
            return AuthenticateResult.Fail("La cuenta no está activa.");

        var claims = new[]
        {
            // Claim estándar y el claim "UsuarioId" usado por GetUsuarioId() del equipo.
            new Claim(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
            new Claim("UsuarioId", sesion.UsuarioId.ToString()),
            new Claim("NombreUsuario", sesion.Usuario.NombreUsuario),
            new Claim("CorreoElectronico", sesion.Usuario.CorreoElectronico),
            new Claim("Rol", sesion.Usuario.Rol.Nombre),
            new Claim("TemaVisual", sesion.Usuario.TemaVisual),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
