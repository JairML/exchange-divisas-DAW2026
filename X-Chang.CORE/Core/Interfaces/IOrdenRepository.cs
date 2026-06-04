using X_Chang.CORE.Core.DTOs.Ordenes;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

// US-004: acceso a datos de órdenes de compra.
public interface IOrdenRepository
{
    Task<(List<OrdenesCompra> Items, int Total)> ObtenerPaginadoAsync(int usuarioId, FiltroOrdenesDto filtro);
    Task<OrdenesCompra?> ObtenerPorIdAsync(int usuarioId, int ordenId);
}
