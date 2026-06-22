namespace X_Chang.CORE.Core.Interfaces;

public interface IMatchingService
{
    Task EjecutarMatchingOrdenAsync(int ordenCompraId);
    Task EjecutarMatchingOfertaAsync(int ofertaVentaId);
}