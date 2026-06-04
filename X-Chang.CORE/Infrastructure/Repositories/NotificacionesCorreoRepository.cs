using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

// US-018: acceso a datos de notificaciones de correo pendientes.
public class NotificacionesCorreoRepository : INotificacionesCorreoRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public NotificacionesCorreoRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificacionesCorreo>> GetPendientesAsync(int limite = 50)
    {
        return await _context.NotificacionesCorreo
            .Where(n => n.EstadoEnvio == "Pendiente")
            .Include(n => n.AdjuntosCorreo)
            .OrderBy(n => n.FechaCreacion)
            .Take(limite)
            .ToListAsync();
    }

    public async Task MarcarEnviadoAsync(int notificacionId)
    {
        var notif = await _context.NotificacionesCorreo.FindAsync(notificacionId);
        if (notif == null) return;

        notif.EstadoEnvio = "Enviado";
        notif.FechaEnvio = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task MarcarErrorAsync(int notificacionId)
    {
        var notif = await _context.NotificacionesCorreo.FindAsync(notificacionId);
        if (notif == null) return;

        notif.EstadoEnvio = "Error";
        await _context.SaveChangesAsync();
    }

    public async Task EncolarAsync(NotificacionesCorreo notif)
    {
        notif.FechaCreacion = DateTime.Now;
        notif.EstadoEnvio = "Pendiente";
        _context.NotificacionesCorreo.Add(notif);
        await _context.SaveChangesAsync();
    }
}
