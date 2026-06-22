using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.DTOs;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Services;

public class OfertaService : IOfertaService
{
    private readonly ExchangeDivisasDbContext _context;
    private readonly IMercadoRepository _mercadoRepository;
    private static readonly string[] EstadosActivos = { "Activa", "Parcialmente ejecutada" };

    public OfertaService(ExchangeDivisasDbContext context, IMercadoRepository mercadoRepository)
    {
        _context = context;
        _mercadoRepository = mercadoRepository;
    }

    public async Task<OfertasActivasResponseDto> ListarOfertasActivasAsync(int usuarioId, FiltroOfertasRequest filtro)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamanoPagina = filtro.TamanoPagina <= 0 ? 10 : filtro.TamanoPagina;

        var query = _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .Where(o => o.UsuarioId == usuarioId && EstadosActivos.Contains(o.Estado));

        if (filtro.Desde.HasValue)
            query = query.Where(o => o.FechaCreacion >= filtro.Desde.Value.Date);

        if (filtro.Hasta.HasValue)
            query = query.Where(o => o.FechaCreacion < filtro.Hasta.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(o => o.Estado == filtro.Estado);

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(o => o.FechaCreacion)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        var ofertas = rows.Select(o => MapOfertaDto(o, o.ParMoneda)).ToList();
        return new OfertasActivasResponseDto(ofertas, total, pagina, tamanoPagina);
    }

    public async Task<OfertaDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaRequest request)
    {
        var resultado = await _mercadoRepository.CrearOfertaVentaAsync(usuarioId, new CrearOfertaVentaRequestDto
        {
            ParMonedaId = request.ParMonedaId,
            CantidadAVender = request.Cantidad,
            PrecioUnitario = request.PrecioUnitario
        });

        var oferta = await _context.OfertasVenta
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
            .FirstAsync(o => o.OfertaVentaId == resultado.OfertaVentaId);

        return MapOfertaDto(oferta, oferta.ParMoneda);
    }

    private static OfertaDto MapOfertaDto(OfertasVenta o, ParesMoneda par) =>
        new(o.OfertaVentaId, o.ParMonedaId,
            par.MonedaOrigen.CodigoIso, par.MonedaDestino.CodigoIso,
            o.CantidadOriginal, o.CantidadVendida, o.CantidadPendiente,
            o.PrecioUnitario, o.TotalEsperado, o.TotalRecibido,
            o.Estado, o.FechaCreacion, o.FechaActualizacion);
}
