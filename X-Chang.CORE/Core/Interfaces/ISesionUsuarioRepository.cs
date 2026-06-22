using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface ISesionUsuarioRepository
    {
        Task<SesionesUsuario?> ObtenerSesionActivaAsync(string tokenSesion);

        Task<SesionesUsuario> CrearSesionAsync(
            int usuarioId,
            string tokenSesion,
            DateTime fechaExpiracion);

        Task<bool> CerrarSesionAsync(string tokenSesion);

        Task<bool> ExisteSesionActivaAsync(string tokenSesion);

        Task CerrarSesionesActivasDeUsuarioAsync(int usuarioId);
    }
}
