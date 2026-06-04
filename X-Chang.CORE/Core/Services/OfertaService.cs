using X_Chang.CORE.Core.DTOs.Common;
using X_Chang.CORE.Core.DTOs.Ofertas;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

// US-004: consulta paginada de ofertas de venta del usuario autenticado.
public class OfertaService : IOfertaService
{
    private readonly IOfertaRepository _repo;

    public OfertaService(IOfertaRepository repo) => _repo = repo;

    public async Task<PagedResultDto<OfertaDto>> ObtenerMisOfertasAsync(int usuarioId, FiltroOfertasDto filtro)
    {
        var (items, total) = await _repo.ObtenerPaginadoAsync(usuarioId, filtro);
        return new PagedResultDto<OfertaDto>(items.Select(Map).ToList(), total, filtro.Pagina, filtro.TamanoPagina);
    }

    public async Task<OfertaDto> ObtenerOfertaAsync(int usuarioId, int ofertaId)
    {
        var oferta = await _repo.ObtenerPorIdAsync(usuarioId, ofertaId)
            ?? throw new InvalidOperationException("Oferta no encontrada.");
        return Map(oferta);
    }

    private static OfertaDto Map(OfertasVenta o) => new(
        o.OfertaVentaId, o.ParMonedaId,
        o.ParMoneda.MonedaOrigen.CodigoIso, o.ParMoneda.MonedaDestino.CodigoIso,
        o.CantidadOriginal, o.CantidadVendida, o.CantidadPendiente,
        o.PrecioUnitario, o.TotalEsperado, o.TotalEsperado - o.TotalRecibido,
        o.Estado, o.FechaCreacion);
}
