using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Catalogos;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class ParesMonedaRepository : IParesMonedaRepository
{
    private readonly ExchangeDivisasDbContext _context;
    private static readonly string[] EstadosActivos = ["Activa", "Parcialmente ejecutada"];

    public ParesMonedaRepository(ExchangeDivisasDbContext context) => _context = context;

    private static MonedaDto MapMoneda(Monedas m) => new()
    {
        MonedaId = m.MonedaId,
        CodigoISO = m.CodigoIso,
        Nombre = m.Nombre,
        Tipo = m.Tipo,
        Activa = m.Activa
    };

    public async Task<List<ParMonedaDto>> ObtenerTodosAsync(bool? activo)
    {
        var pares = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .Where(p => activo == null || p.Activo == activo.Value)
            .OrderBy(p => p.MonedaOrigen.CodigoIso)
            .ThenBy(p => p.MonedaDestino.CodigoIso)
            .ToListAsync();

        var parIds = pares.Select(p => p.ParMonedaId).ToList();

        // Best bid per par (max buy price from active orders)
        var bids = await _context.OrdenesCompra
            .Where(o => parIds.Contains(o.ParMonedaId)
                && EstadosActivos.Contains(o.Estado)
                && o.CantidadPendiente > 0)
            .GroupBy(o => o.ParMonedaId)
            .Select(g => new { ParMonedaId = g.Key, Precio = g.Max(o => o.PrecioUnitario) })
            .ToDictionaryAsync(x => x.ParMonedaId, x => (decimal?)x.Precio);

        // Best ask per par (min sell price from active offers)
        var asks = await _context.OfertasVenta
            .Where(o => parIds.Contains(o.ParMonedaId)
                && EstadosActivos.Contains(o.Estado)
                && o.CantidadPendiente > 0)
            .GroupBy(o => o.ParMonedaId)
            .Select(g => new { ParMonedaId = g.Key, Precio = g.Min(o => o.PrecioUnitario) })
            .ToDictionaryAsync(x => x.ParMonedaId, x => (decimal?)x.Precio);

        return pares.Select(p =>
        {
            var bid = bids.GetValueOrDefault(p.ParMonedaId);
            var ask = asks.GetValueOrDefault(p.ParMonedaId);
            return new ParMonedaDto
            {
                ParMonedaId = p.ParMonedaId,
                MonedaOrigen = MapMoneda(p.MonedaOrigen),
                MonedaDestino = MapMoneda(p.MonedaDestino),
                Activo = p.Activo,
                MejorPrecioCompra = bid,
                MejorPrecioVenta = ask,
                Spread = bid.HasValue && ask.HasValue ? ask - bid : null
            };
        }).ToList();
    }

    public async Task<ParMonedaDetalleDto?> ObtenerDetalleAsync(int parMonedaId)
    {
        var par = await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId);

        if (par == null) return null;

        // Order book — grouped by price level, sorted in memory to avoid EF GroupBy+OrderBy issues
        var compras = (await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId
                && EstadosActivos.Contains(o.Estado)
                && o.CantidadPendiente > 0)
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelLibroDto
            {
                Precio = g.Key,
                Cantidad = g.Sum(o => o.CantidadPendiente),
                Total = g.Sum(o => o.CantidadPendiente * o.PrecioUnitario)
            })
            .ToListAsync())
            .OrderByDescending(x => x.Precio)
            .ToList();

        var ventas = (await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId
                && EstadosActivos.Contains(o.Estado)
                && o.CantidadPendiente > 0)
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelLibroDto
            {
                Precio = g.Key,
                Cantidad = g.Sum(o => o.CantidadPendiente),
                Total = g.Sum(o => o.CantidadPendiente * o.PrecioUnitario)
            })
            .ToListAsync())
            .OrderBy(x => x.Precio)
            .ToList();

        var bid = compras.Count > 0 ? compras[0].Precio : (decimal?)null;
        var ask = ventas.Count > 0 ? ventas[0].Precio : (decimal?)null;

        return new ParMonedaDetalleDto
        {
            ParMonedaId = par.ParMonedaId,
            MonedaOrigen = MapMoneda(par.MonedaOrigen),
            MonedaDestino = MapMoneda(par.MonedaDestino),
            Activo = par.Activo,
            MejorPrecioCompra = bid,
            MejorPrecioVenta = ask,
            Spread = bid.HasValue && ask.HasValue ? ask - bid : null,
            LibroOrdenes = new LibroOrdenesDto { Compras = compras, Ventas = ventas }
        };
    }
}
