using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.API.Helpers;

public class NotificacionesBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificacionesBackgroundService> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(2);

    public NotificacionesBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificacionesBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var servicio = scope.ServiceProvider.GetRequiredService<INotificacionesCorreoService>();
                await servicio.ProcesarPendientesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar notificaciones pendientes.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}
