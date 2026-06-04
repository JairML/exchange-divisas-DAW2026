using BCrypt.Net;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Auth;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly ISesionUsuarioRepository _sesionRepo;

    public AuthService(IAuthRepository authRepo, ISesionUsuarioRepository sesionRepo)
    {
        _authRepo = authRepo;
        _sesionRepo = sesionRepo;
    }

    public async Task<ResultadoOperacion<AuthResponseDto>> RegistrarAsync(RegistroRequestDto dto)
    {
        if (await _authRepo.ExisteNombreUsuarioAsync(dto.NombreUsuario))
            return ResultadoOperacion<AuthResponseDto>.Error("El nombre de usuario ya está en uso.");

        if (await _authRepo.ExisteCorreoAsync(dto.CorreoElectronico))
            return ResultadoOperacion<AuthResponseDto>.Error("El correo electrónico ya está registrado.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = await _authRepo.CrearUsuarioConBilleteraAsync(
            dto.NombreUsuario, dto.CorreoElectronico, passwordHash, dto.PaisId);

        await _authRepo.RegistrarAccesoAsync(
            usuario.UsuarioId, exitoso: true, metodoIngreso: "NombreUsuario", mensaje: "Registro exitoso");

        var token = Guid.NewGuid().ToString("N");
        await _sesionRepo.CrearSesionAsync(usuario.UsuarioId, token, DateTime.Now.AddHours(8));

        return ResultadoOperacion<AuthResponseDto>.Ok(new AuthResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            Rol = usuario.Rol.Nombre,
            Pais = usuario.Pais.Nombre,
            Estado = usuario.Estado,
            TemaVisual = usuario.TemaVisual,
            FechaRegistro = usuario.FechaRegistro,
            TokenSesion = token
        });
    }

    public async Task<ResultadoOperacion<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        var usuario = await _authRepo.ObtenerPorCredencialAsync(dto.Credencial);

        if (usuario == null)
            return ResultadoOperacion<AuthResponseDto>.Error("Credenciales incorrectas.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            await _authRepo.RegistrarAccesoAsync(
                usuario.UsuarioId, exitoso: false, metodoIngreso: "NombreUsuario", mensaje: "Contraseña incorrecta");

            return ResultadoOperacion<AuthResponseDto>.Error("Credenciales incorrectas.");
        }

        if (usuario.Estado != "Activo")
        {
            await _authRepo.RegistrarAccesoAsync(
                usuario.UsuarioId, exitoso: false, metodoIngreso: "NombreUsuario", mensaje: $"Cuenta {usuario.Estado}");

            return ResultadoOperacion<AuthResponseDto>.Error($"La cuenta está {usuario.Estado.ToLower()}.");
        }

        await _authRepo.ActualizarUltimoAccesoAsync(usuario.UsuarioId);
        await _authRepo.RegistrarAccesoAsync(
            usuario.UsuarioId, exitoso: true, metodoIngreso: "NombreUsuario", mensaje: "Login exitoso");

        var token = Guid.NewGuid().ToString("N");
        await _sesionRepo.CrearSesionAsync(usuario.UsuarioId, token, DateTime.Now.AddHours(8));

        return ResultadoOperacion<AuthResponseDto>.Ok(new AuthResponseDto
        {
            UsuarioId = usuario.UsuarioId,
            NombreUsuario = usuario.NombreUsuario,
            CorreoElectronico = usuario.CorreoElectronico,
            Rol = usuario.Rol.Nombre,
            Pais = usuario.Pais.Nombre,
            Estado = usuario.Estado,
            TemaVisual = usuario.TemaVisual,
            FechaRegistro = usuario.FechaRegistro,
            TokenSesion = token
        });
    }

    public async Task<ResultadoOperacion<bool>> LogoutAsync(string tokenSesion)
    {
        var cerrado = await _sesionRepo.CerrarSesionAsync(tokenSesion);

        return cerrado
            ? ResultadoOperacion<bool>.Ok(true)
            : ResultadoOperacion<bool>.Error("Sesión no encontrada o ya cerrada.");
    }
}
