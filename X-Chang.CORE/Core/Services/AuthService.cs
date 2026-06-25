using System.Security.Cryptography;
using BC = BCrypt.Net.BCrypt;
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
    private readonly IEmailService _emailService;

    public AuthService(
        IAuthRepository authRepo,
        ISesionUsuarioRepository sesionRepo,
        IOptions<SessionSettings> settings,
        IEmailService emailService)
    {
        _authRepo = authRepo;
        _sesionRepo = sesionRepo;
        _settings = settings.Value;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto request)
    {
        if (request.Password != request.ConfirmarPassword)
            throw new InvalidOperationException("Las contraseñas no coinciden.");

        if (await _authRepo.ExisteCorreoAsync(request.CorreoElectronico))
            throw new InvalidOperationException("Correo ya registrado.");

        if (await _authRepo.ExisteNombreUsuarioAsync(request.NombreUsuario))
            throw new InvalidOperationException("Usuario ya registrado.");

        var rol = await _authRepo.ObtenerRolPorNombreAsync("Usuario")
            ?? throw new InvalidOperationException("Rol 'Usuario' no configurado.");

        _ = await _authRepo.ObtenerPaisAsync(request.PaisId)
            ?? throw new InvalidOperationException("País no encontrado.");

        var usuario = await _authRepo.CrearUsuarioAsync(new Usuarios
        {
            NombreUsuario = request.NombreUsuario,
            CorreoElectronico = request.CorreoElectronico,
            PasswordHash = BC.HashPassword(request.Password),
            RolId = rol.RolId,
            PaisId = request.PaisId,
            TemaVisual = "Claro",
            Estado = "Activo",
            FechaRegistro = DateTime.UtcNow
        });

        await _authRepo.CrearBilleteraAsync(new Billeteras { UsuarioId = usuario.UsuarioId, FechaCreacion = DateTime.UtcNow });

        await _emailService.EnviarAsync(
            usuario.CorreoElectronico,
            "Bienvenido a X-Chang",
            $"""
            <h2>¡Bienvenido a X-Chang, {usuario.NombreUsuario}!</h2>
            <p>Tu cuenta ha sido creada exitosamente. Ya puedes iniciar sesión y comenzar a operar en nuestra plataforma de intercambio de divisas.</p>
            <p>Si tienes alguna consulta, no dudes en contactarnos.</p>
            <br/>
            <p>El equipo de X-Chang</p>
            """);

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
        var exito = usuario != null && BC.Verify(request.Password, usuario.PasswordHash);

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

        if (!exito || usuario == null)
            throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");

        if (usuario.Estado != "Activo")
            throw new UnauthorizedAccessException($"La cuenta está {usuario.Estado.ToLower()}.");

        await _authRepo.ActualizarFechaAccesoAsync(usuario.UsuarioId);
        return await GenerarResponseAsync(usuario, usuario.Rol.Nombre);
    }

    public async Task LogoutAsync(int usuarioId, string token) =>
        await _sesionRepo.CerrarSesionAsync(token);

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var sesion = await _sesionRepo.ObtenerSesionActivaAsync(request.Token);

        if (sesion == null || sesion.FechaExpiracion < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Token inválido o expirado.");

        if (sesion.Usuario.Estado != "Activo")
            throw new UnauthorizedAccessException("La cuenta no está activa.");

        await _sesionRepo.CerrarSesionAsync(request.Token);
        return await GenerarResponseAsync(sesion.Usuario, sesion.Usuario.Rol.Nombre);
    }

    private async Task<AuthResponseDto> GenerarResponseAsync(Usuarios usuario, string rolNombre)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expira = DateTime.UtcNow.AddDays(_settings.ExpiresInDays);

        await _sesionRepo.CerrarSesionesActivasDeUsuarioAsync(usuario.UsuarioId);
        await _sesionRepo.CrearSesionAsync(usuario.UsuarioId, token, expira);

        return new AuthResponseDto(token, expira, new UsuarioInfoDto(
            usuario.UsuarioId, usuario.NombreUsuario, usuario.CorreoElectronico,
            rolNombre, usuario.TemaVisual, usuario.Estado));
    }
}
