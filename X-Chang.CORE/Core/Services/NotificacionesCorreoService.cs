using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

// US-018: procesa las notificaciones pendientes y las envía por correo.
public class NotificacionesCorreoService : INotificacionesCorreoService
{
    private readonly INotificacionesCorreoRepository _repo;
    private readonly IEmailService _emailService;
    private readonly IUsuarioRepository _usuarioRepo;

    public NotificacionesCorreoService(
        INotificacionesCorreoRepository repo,
        IEmailService emailService,
        IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _emailService = emailService;
        _usuarioRepo = usuarioRepo;
    }

    public async Task ProcesarPendientesAsync()
    {
        var pendientes = await _repo.GetPendientesAsync();

        foreach (var notif in pendientes)
        {
            var enviado = await _emailService.EnviarAsync(
                notif.CorreoDestino,
                notif.Asunto,
                notif.Cuerpo,
                notif.AdjuntosCorreo);

            if (enviado)
                await _repo.MarcarEnviadoAsync(notif.NotificacionId);
            else
                await _repo.MarcarErrorAsync(notif.NotificacionId);
        }
    }

    public async Task EncolarAsync(
        int usuarioId,
        string tipoEvento,
        string asunto,
        string cuerpo,
        string? referenciaTipo = null,
        int? referenciaId = null)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId);
        if (usuario == null) return;

        await _repo.EncolarAsync(new NotificacionesCorreo
        {
            UsuarioId = usuarioId,
            CorreoDestino = usuario.CorreoElectronico,
            TipoEvento = tipoEvento,
            Asunto = asunto,
            Cuerpo = cuerpo,
            ReferenciaTipo = referenciaTipo,
            ReferenciaId = referenciaId
        });
    }
}
