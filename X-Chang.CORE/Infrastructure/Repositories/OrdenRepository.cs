using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Ordenes;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

// US-004: acceso a datos de órdenes de compra.
public class OrdenRepository : IOrdenRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public OrdenRepository(ExchangeDivisasDbContext context) => _context = context;

    public async Task<(List<OrdenesCompra> Items, int Total)> ObtenerPaginadoAsync(
        int usuarioId, FiltroOrdenesDto filtro)
    {
        var query = _context.OrdenesCompra
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

    public Task<OrdenesCompra?> ObtenerPorIdAsync(int usuarioId, int ordenId) =>
        _context.OrdenesCompra
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenId && o.UsuarioId == usuarioId);
}
