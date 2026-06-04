using X_Chang.CORE.Core.DTOs.Ofertas;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

// US-004: acceso a datos de ofertas de venta.
public interface IOfertaRepository
{
    Task<(List<OfertasVenta> Items, int Total)> ObtenerPaginadoAsync(int usuarioId, FiltroOfertasDto filtro);
    Task<OfertasVenta?> ObtenerPorIdAsync(int usuarioId, int ofertaId);
}
