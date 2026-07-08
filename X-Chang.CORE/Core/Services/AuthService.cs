using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using X_Chang.CORE.Core.DTOs.Auth;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Settings;

namespace X_Chang.CORE.Core.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly ISesionUsuarioRepository _sesionRepo;
    private readonly SessionSettings _settings;

    public AuthService(
        IAuthRepository authRepo,
        ISesionUsuarioRepository sesionRepo,
        IOptions<SessionSettings> settings,
        IEmailService emailService)
    {
        _authRepo = authRepo;
        _sesionRepo = sesionRepo;
        _settings = settings.Value;
    }

    public async Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto request)
    {
        if (request.Password != request.ConfirmarPassword)
            throw new InvalidOperationException("No coinciden");

        if (await _authRepo.ExisteCorreoAsync(request.CorreoElectronico))
            throw new InvalidOperationException("Correo ya registrado");

        if (await _authRepo.ExisteNombreUsuarioAsync(request.NombreUsuario))
            throw new InvalidOperationException("Usuario ya registrado");

        var rol = await _authRepo.ObtenerRolPorNombreAsync("USU")
            ?? throw new InvalidOperationException("Rol USU no configurado.");

        _ = await _authRepo.ObtenerPaisAsync(request.PaisId)
            ?? throw new InvalidOperationException("Seleccione un país");

        var usuario = await _authRepo.CrearUsuarioAsync(new Usuarios
        {
            NombreUsuario = request.NombreUsuario.Trim(),
            CorreoElectronico = request.CorreoElectronico.Trim(),
            PasswordHash = HashSha256(request.Password),
            RolId = rol.RolId,
            PaisId = request.PaisId,
            TemaVisual = "Oscuro",
            Estado = "Activo",
            FechaRegistro = DateTime.UtcNow,
            Telefono = request.Telefono,
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento
        });

        await _authRepo.CrearBilleteraAsync(new Billeteras
        {
            UsuarioId = usuario.UsuarioId,
            FechaCreacion = DateTime.UtcNow
        });

        await _authRepo.RegistrarAccesoAsync(new AccesosUsuario
        {
            UsuarioId = usuario.UsuarioId,
            FechaAcceso = DateTime.UtcNow,
            Exitoso = true,
            MetodoIngreso = "NombreUsuario",
            MensajeResultado = "Registro exitoso"
        });

        return await GenerarResponseAsync(usuario, rol.Nombre);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var id = request.IdentificadorAcceso.Trim();
        var metodoIngreso = id.Contains('@') ? "CorreoElectronico" : "NombreUsuario";
        var usuario = await _authRepo.BuscarPorIdentificadorAsync(id);
        var hash = HashSha256(request.Password);
        var exito = usuario != null && string.Equals(usuario.PasswordHash, hash, StringComparison.OrdinalIgnoreCase);

        if (usuario != null)
        {
            await _authRepo.RegistrarAccesoAsync(new AccesosUsuario
            {
                UsuarioId = usuario.UsuarioId,
                FechaAcceso = DateTime.UtcNow,
                Exitoso = exito,
                MetodoIngreso = metodoIngreso,
                MensajeResultado = exito ? "Login exitoso" : "Credenciales inválidas"
            });
        }

        if (usuario == null || !exito)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        if (usuario.Estado != "Activo" && usuario.Estado != "Restringido")
            throw new UnauthorizedAccessException("Credenciales inválidas");

        await _authRepo.ActualizarFechaAccesoAsync(usuario.UsuarioId);
        return await GenerarResponseAsync(usuario, usuario.Rol.Nombre);
    }

    public async Task LogoutAsync(int usuarioId, string token) =>
        await _sesionRepo.CerrarSesionAsync(token);

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var sesion = await _sesionRepo.ObtenerSesionActivaAsync(request.Token);

        if (sesion == null || sesion.FechaExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        if (sesion.Usuario.Estado != "Activo" && sesion.Usuario.Estado != "Restringido")
            throw new UnauthorizedAccessException("Credenciales inválidas");

        await _sesionRepo.CerrarSesionAsync(request.Token);
        return await GenerarResponseAsync(sesion.Usuario, sesion.Usuario.Rol.Nombre);
    }

    private async Task<AuthResponseDto> GenerarResponseAsync(Usuarios usuario, string rolNombre)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expira = DateTime.UtcNow.AddDays(_settings.ExpiresInDays <= 0 ? 7 : _settings.ExpiresInDays);

        await _sesionRepo.CerrarSesionesActivasDeUsuarioAsync(usuario.UsuarioId);
        await _sesionRepo.CrearSesionAsync(usuario.UsuarioId, token, expira);

        return new AuthResponseDto(token, expira, new UsuarioInfoDto(
            usuario.UsuarioId,
            usuario.NombreUsuario,
            usuario.CorreoElectronico,
            MapRol(rolNombre),
            usuario.TemaVisual,
            usuario.Estado));
    }

    private static string MapRol(string rolNombre) => rolNombre == "ADM" ? "Administrador" : "Usuario";

    private static string HashSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
