using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface IAuthRepository
{
    Task<bool> ExisteCorreoAsync(string correo);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<Roles?> ObtenerRolPorNombreAsync(string nombre);
    Task<Paises?> ObtenerPaisAsync(int paisId);
    Task<Usuarios> CrearUsuarioAsync(Usuarios usuario);
    Task CrearBilleteraAsync(Billeteras billetera);
    Task RegistrarAccesoAsync(AccesosUsuario acceso);
    Task<Usuarios?> BuscarPorIdentificadorAsync(string identificador);
    Task ActualizarFechaAccesoAsync(int usuarioId);
}
