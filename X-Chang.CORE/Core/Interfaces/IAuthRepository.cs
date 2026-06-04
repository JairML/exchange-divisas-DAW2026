using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

// US-001 / US-002: acceso a datos de autenticación.
public interface IAuthRepository
{
    // US-001
    Task<bool> ExisteCorreoAsync(string correo);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<Roles?> ObtenerRolPorNombreAsync(string nombre);
    Task<Paises?> ObtenerPaisAsync(int paisId);
    Task<Usuarios> CrearUsuarioAsync(Usuarios usuario);
    Task CrearBilleteraAsync(Billeteras billetera);
    Task RegistrarAccesoAsync(AccesosUsuario acceso);

    // US-002
    Task<Usuarios?> BuscarPorIdentificadorAsync(string identificador);
    Task ActualizarFechaAccesoAsync(int usuarioId);
}
