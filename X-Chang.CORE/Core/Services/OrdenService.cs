using X_Chang.CORE.Core.DTOs.Common;
using X_Chang.CORE.Core.DTOs.Ordenes;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

// US-004: consulta paginada de órdenes de compra del usuario autenticado.
public class OrdenService : IOrdenService
{
    private readonly IOrdenRepository _repo;

    public OrdenService(IOrdenRepository repo) => _repo = repo;

    public async Task<PagedResultDto<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId, FiltroOrdenesDto filtro)
    {
        var (items, total) = await _repo.ObtenerPaginadoAsync(usuarioId, filtro);
        return new PagedResultDto<OrdenDto>(items.Select(Map).ToList(), total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId)
    {
        var orden = await _repo.ObtenerPorIdAsync(usuarioId, ordenId)
            ?? throw new InvalidOperationException("Orden no encontrada.");
        return Map(orden);
    }

    private static OrdenDto Map(OrdenesCompra o) => new(
        o.OrdenCompraId, o.ParMonedaId,
        o.ParMoneda.MonedaOrigen.CodigoIso, o.ParMoneda.MonedaDestino.CodigoIso,
        o.CantidadOriginal, o.CantidadObtenida, o.CantidadPendiente,
        o.PrecioUnitario, o.TotalComprometido, o.TotalComprometido - o.TotalEjecutado,
        o.Estado, o.FechaCreacion);
}
