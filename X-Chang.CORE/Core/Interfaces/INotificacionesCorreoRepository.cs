using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface INotificacionesCorreoRepository
{
    Task<IEnumerable<NotificacionesCorreo>> GetPendientesAsync(int limite = 50);
    Task MarcarEnviadoAsync(int notificacionId);
    Task MarcarErrorAsync(int notificacionId);
    Task EncolarAsync(NotificacionesCorreo notif);
}
