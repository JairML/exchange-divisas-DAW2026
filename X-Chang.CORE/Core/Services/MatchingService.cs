using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class MatchingService : IMatchingService
{
    public Task EjecutarMatchingOrdenAsync(int ordenCompraId)
    {
        // El cruce de órdenes se ejecuta dentro de MercadoRepository.CrearOrdenCompraAsync,
        // para mantener en una sola transacción el débito de billetera, ejecuciones,
        // historial, notificaciones y sincronización del par espejo.
        return Task.CompletedTask;
    }

    public Task EjecutarMatchingOfertaAsync(int ofertaVentaId)
    {
        // El cruce de ofertas se ejecuta dentro de MercadoRepository.CrearOfertaVentaAsync,
        // para mantener en una sola transacción el débito de billetera, ejecuciones,
        // historial, notificaciones y sincronización del par espejo.
        return Task.CompletedTask;
    }
}
