using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface IAuthRepository
{
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<bool> ExisteCorreoAsync(string correo);
    Task<Usuarios?> ObtenerPorCredencialAsync(string credencial);
    Task<Usuarios> CrearUsuarioConBilleteraAsync(string nombreUsuario, string correo, string passwordHash, int paisId);
    Task RegistrarAccesoAsync(int usuarioId, bool exitoso, string metodoIngreso, string? mensaje = null);
    Task ActualizarUltimoAccesoAsync(int usuarioId);
}
