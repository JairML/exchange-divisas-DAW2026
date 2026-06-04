using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Ofertas;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

// US-004: acceso a datos de ofertas de venta.
public class OfertaRepository : IOfertaRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public OfertaRepository(ExchangeDivisasDbContext context) => _context = context;

    public async Task<(List<OfertasVenta> Items, int Total)> ObtenerPaginadoAsync(
        int usuarioId, FiltroOfertasDto filtro)
    {
        var query = _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId)
            .AsQueryable();

        if (filtro.Desde.HasValue) query = query.Where(o => o.FechaCreacion >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) query = query.Where(o => o.FechaCreacion <= filtro.Hasta.Value);

        query = query.OrderByDescending(o => o.FechaCreacion);

        var total = await query.CountAsync();
        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync();

        return (items, total);
    }

    public Task<OfertasVenta?> ObtenerPorIdAsync(int usuarioId, int ofertaId) =>
        _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaId && o.UsuarioId == usuarioId);
}
