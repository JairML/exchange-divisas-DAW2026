using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Interfaces;

namespace X_Chang.CORE.Services;

public class OrdenService : IOrdenService
{
    private readonly ExchangeDivisasDbContext _context;

    public OrdenService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId)
    {
        var compras = await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(
                g.Key,
                g.Sum(o => o.CantidadPendiente),
                g.Count()))
            .OrderByDescending(n => n.Precio)
            .Take(20)
            .ToListAsync();

        var ventas = await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(
                g.Key,
                g.Sum(o => o.CantidadPendiente),
                g.Count()))
            .OrderBy(n => n.Precio)
            .Take(20)
            .ToListAsync();

        return new LibroOrdenesDto(compras, ventas);
    }

    public async Task<LibroOrdenesDetalleDto> ObtenerLibroOrdenesDetalleAsync(int parMonedaId, int limite = 10)
    {
        var compras = await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .OrderByDescending(o => o.PrecioUnitario)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(
                o.OrdenCompraId, o.CantidadPendiente, o.PrecioUnitario, o.FechaCreacion))
            .ToListAsync();

        var ventas = await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId && o.Estado == "Activa")
            .OrderBy(o => o.PrecioUnitario)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(
                o.OfertaVentaId, o.CantidadPendiente, o.PrecioUnitario, o.FechaCreacion))
            .ToListAsync();

        return new LibroOrdenesDetalleDto(compras, ventas);
    }
}