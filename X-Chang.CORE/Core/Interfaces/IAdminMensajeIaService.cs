using X_Chang.CORE.Core.DTOs.GestionUsuarios;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IAdminMensajeIaService
    {
        Task<string> GenerarMensajeAsync(
            UsuarioAdminDetalleDto usuario,
            string tipoAccion,
            string? mensajeActual,
            CancellationToken cancellationToken = default);
    }
}
