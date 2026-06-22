using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Infrastructure.Data;

using MercadoCrearOrdenCompraRequestDto = X_Chang.CORE.Core.DTOs.Mercado.CrearOrdenCompraRequestDto;

namespace X_Chang.CORE.Services;

public class OrdenService : IOrdenService
{
    private readonly ExchangeDivisasDbContext _context;
    private readonly IMercadoRepository _mercadoRepository;

    private static readonly string[] EstadosActivos =
    {
        "Activa",
        "Parcialmente ejecutada"
    };

    public OrdenService(
        ExchangeDivisasDbContext context,
        IMercadoRepository mercadoRepository)
    {
        _context = context;
        _mercadoRepository = mercadoRepository;
    }

    private static OrdenDto MapOrdenDto(OrdenesCompra o, ParesMoneda par)
    {
        return new OrdenDto(
            o.OrdenCompraId,
            o.ParMonedaId,
            par.MonedaOrigen.CodigoIso,
            par.MonedaDestino.CodigoIso,
            o.CantidadOriginal,
            o.CantidadObtenida,
            o.CantidadPendiente,
            o.PrecioUnitario,
            o.TotalComprometido,
            o.TotalEjecutado,
            o.Estado,
            o.FechaCreacion,
            o.FechaActualizacion);
    }

    public async Task<X_Chang.CORE.DTOs.LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId)
    {
        var compras = await _context.OrdenesCompra
            .Where(o =>
                o.ParMonedaId == parMonedaId &&
                EstadosActivos.Contains(o.Estado) &&
                o.CantidadPendiente > 0)
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(
                g.Key,
                g.Sum(o => o.CantidadPendiente),
                g.Count()))
            .OrderByDescending(n => n.Precio)
            .Take(20)
            .ToListAsync();

        var ventas = await _context.OfertasVenta
            .Where(o =>
                o.ParMonedaId == parMonedaId &&
                EstadosActivos.Contains(o.Estado) &&
                o.CantidadPendiente > 0)
            .GroupBy(o => o.PrecioUnitario)
            .Select(g => new NivelOrdenDto(
                g.Key,
                g.Sum(o => o.CantidadPendiente),
                g.Count()))
            .OrderBy(n => n.Precio)
            .Take(20)
            .ToListAsync();

        return new X_Chang.CORE.DTOs.LibroOrdenesDto(compras, ventas);
    }

    public async Task<LibroOrdenesDetalleDto> ObtenerLibroOrdenesDetalleAsync(
        int parMonedaId,
        int limite = 10)
    {
        if (limite <= 0)
            limite = 10;

        var compras = await _context.OrdenesCompra
            .Where(o =>
                o.ParMonedaId == parMonedaId &&
                EstadosActivos.Contains(o.Estado) &&
                o.CantidadPendiente > 0)
            .OrderByDescending(o => o.PrecioUnitario)
            .ThenBy(o => o.FechaCreacion)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(
                o.OrdenCompraId,
                o.CantidadPendiente,
                o.PrecioUnitario,
                o.FechaCreacion))
            .ToListAsync();

        var ventas = await _context.OfertasVenta
            .Where(o =>
                o.ParMonedaId == parMonedaId &&
                EstadosActivos.Contains(o.Estado) &&
                o.CantidadPendiente > 0)
            .OrderBy(o => o.PrecioUnitario)
            .ThenBy(o => o.FechaCreacion)
            .Take(limite)
            .Select(o => new LibroOrdenEntradaDto(
                o.OfertaVentaId,
                o.CantidadPendiente,
                o.PrecioUnitario,
                o.FechaCreacion))
            .ToListAsync();

        return new LibroOrdenesDetalleDto(compras, ventas);
    }

    public async Task<OrdenDto?> ObtenerOrdenPorIdAsync(int usuarioId, int ordenId)
    {
        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o =>
                o.OrdenCompraId == ordenId &&
                o.UsuarioId == usuarioId);

        return orden == null
            ? null
            : MapOrdenDto(orden, orden.ParMoneda);
    }

    public async Task<OrdenesActivasResponseDto> ListarOrdenesActivasAsync(
        int usuarioId,
        FiltroOrdenesRequest filtro)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamanoPagina = filtro.TamanoPagina <= 0 ? 10 : filtro.TamanoPagina;

        var query = _context.OrdenesCompra
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaDestino)
            .Where(o =>
                o.UsuarioId == usuarioId &&
                EstadosActivos.Contains(o.Estado));

        if (filtro.Desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= filtro.Desde.Value.Date);

        if (filtro.Hasta.HasValue)
            query = query.Where(o => o.FechaCreacion < filtro.Hasta.Value.Date.AddDays(1));

        var total = await query.CountAsync();

        var rows = await query
            .OrderByDescending(o => o.FechaCreacion)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        var ordenes = rows
            .Select(o => MapOrdenDto(o, o.ParMoneda))
            .ToList();

        return new OrdenesActivasResponseDto(
            ordenes,
            total,
            pagina,
            tamanoPagina);
    }

    public async Task<OrdenDto> CrearOrdenCompraAsync(
        int usuarioId,
        CrearOrdenRequest request)
    {
        var resultado = await _mercadoRepository.CrearOrdenCompraAsync(
            usuarioId,
            new MercadoCrearOrdenCompraRequestDto
            {
                ParMonedaId = request.ParMonedaId,
                CantidadAObtener = request.Cantidad,
                PrecioUnitario = request.PrecioUnitario
            });

        var orden = await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaDestino)
            .FirstAsync(o => o.OrdenCompraId == resultado.OrdenCompraId);

        return MapOrdenDto(orden, orden.ParMoneda);
    }
}