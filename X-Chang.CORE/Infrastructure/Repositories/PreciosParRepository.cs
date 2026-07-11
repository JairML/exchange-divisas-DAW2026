using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Precios;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class PreciosParRepository : IPreciosParRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        private static readonly string[] EstadosActivos = { "Activa", "Parcialmente ejecutada" };

        public PreciosParRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Monedas>> ObtenerMonedasSoportadasAsync(IEnumerable<string>? codigosIso = null)
        {
            var query = _context.Monedas
                .AsNoTracking()
                .Where(m => m.Activa)
                .AsQueryable();

            if (codigosIso != null)
            {
                var codigos = codigosIso
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim().ToUpper())
                    .Distinct()
                    .ToList();

                if (codigos.Any())
                {
                    query = query.Where(m => codigos.Contains(m.CodigoIso));
                }
            }

            return await query
                .OrderBy(m => m.CodigoIso)
                .ToListAsync();
        }

        public async Task<List<ParesMoneda>> ObtenerTodosParesAsync()
        {
            return await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .ToListAsync();
        }

        public async Task<Dictionary<int, decimal>> ObtenerMayoresPreciosCompraAsync()
        {
            return await _context.OrdenesCompra
                .Where(o => EstadosActivos.Contains(o.Estado)
                    && !(_context.OfertasVenta.Any(v => v.OrdenCompraEspejoId == o.OrdenCompraId)
                        && !_context.MovimientosBilletera.Any(m => m.ReferenciaId == o.OrdenCompraId
                            && (m.ReferenciaTipo == "OrdenCompra" || m.ReferenciaTipo == "ordenescompra"))))
                .GroupBy(o => o.ParMonedaId)
                .Select(g => new { g.Key, Max = g.Max(o => o.PrecioUnitario) })
                .ToDictionaryAsync(x => x.Key, x => x.Max);
        }

        public async Task<Dictionary<int, decimal>> ObtenerMenoresPreciosVentaAsync()
        {
            return await _context.OfertasVenta
                .Where(o => EstadosActivos.Contains(o.Estado)
                    && !(o.OrdenCompraEspejoId != null
                        && !_context.MovimientosBilletera.Any(m => m.ReferenciaId == o.OfertaVentaId
                            && (m.ReferenciaTipo == "OfertaVenta" || m.ReferenciaTipo == "ofertasventa"))))
                .GroupBy(o => o.ParMonedaId)
                .Select(g => new { g.Key, Min = g.Min(o => o.PrecioUnitario) })
                .ToDictionaryAsync(x => x.Key, x => x.Min);
        }

        public async Task<Dictionary<int, decimal>> ObtenerVolumenesPorParAsync()
        {
            return await _context.HistoricoPreciosPar
                .GroupBy(h => h.ParMonedaId)
                .Select(g => new { g.Key, Vol = g.Sum(h => h.VolumenCompra + h.VolumenVenta) })
                .ToDictionaryAsync(x => x.Key, x => x.Vol);
        }

        public async Task<Dictionary<int, DateTime>> ObtenerFechaTransaccionPorParUsuarioAsync(int usuarioId)
        {
            return await _context.HistorialTransacciones
                .Where(h => h.UsuarioId == usuarioId && h.ParMonedaId != null)
                .GroupBy(h => h.ParMonedaId!.Value)
                .Select(g => new { g.Key, MaxFecha = g.Max(h => h.FechaHora) })
                .ToDictionaryAsync(x => x.Key, x => x.MaxFecha);
        }

        public async Task<List<PuntoSerieHistoricaDto>> ObtenerSerieHistoricaAsync(
    int parMonedaId, DateTime? desde)
        {
            var query = _context.HistoricoPreciosPar
                .AsNoTracking()
                .Where(h => h.ParMonedaId == parMonedaId);

            if (desde.HasValue)
            {
                var desdeUtc = DateTime.SpecifyKind(desde.Value, DateTimeKind.Utc);

                query = query.Where(h =>
                    (h.SnapshotMinuto ?? h.FechaRegistro) >= desdeUtc);
            }

            return await query
                .OrderBy(h => h.SnapshotMinuto ?? h.FechaRegistro)
                .Select(h => new PuntoSerieHistoricaDto
                {
                    FechaHora = h.SnapshotMinuto ?? h.FechaRegistro,
                    MayorPrecioCompra = h.MayorPrecioCompra,
                    MenorPrecioVenta = h.MenorPrecioVenta,
                    Margen =
                        h.MayorPrecioCompra.HasValue && h.MenorPrecioVenta.HasValue
                            ? h.MenorPrecioVenta.Value - h.MayorPrecioCompra.Value
                            : null
                })
                .ToListAsync();
        }

        public async Task<string?> ObtenerMonedaPrincipalUsuarioAsync(int usuarioId)
        {
            return await _context.Usuarios
                .Where(u => u.UsuarioId == usuarioId)
                .Select(u => u.Pais.Moneda.CodigoIso)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> ObtenerParMasRecienteActivoUsuarioAsync(int usuarioId)
        {
            var ordenReciente = await _context.OrdenesCompra
                .Where(o => o.UsuarioId == usuarioId && EstadosActivos.Contains(o.Estado))
                .OrderByDescending(o => o.FechaCreacion)
                .Select(o => new { o.ParMonedaId, o.FechaCreacion })
                .FirstOrDefaultAsync();

            var ofertaReciente = await _context.OfertasVenta
                .Where(o => o.UsuarioId == usuarioId && EstadosActivos.Contains(o.Estado))
                .OrderByDescending(o => o.FechaCreacion)
                .Select(o => new { o.ParMonedaId, o.FechaCreacion })
                .FirstOrDefaultAsync();

            if (ordenReciente == null && ofertaReciente == null)
                return null;
            if (ordenReciente == null)
                return ofertaReciente!.ParMonedaId;
            if (ofertaReciente == null)
                return ordenReciente.ParMonedaId;

            return ordenReciente.FechaCreacion >= ofertaReciente.FechaCreacion
                ? ordenReciente.ParMonedaId
                : ofertaReciente.ParMonedaId;
        }

        public async Task<int?> ObtenerParMonedaIdAsync(int monedaOrigenId, int monedaDestinoId)
        {
            return await _context.ParesMoneda
                .Where(p => p.MonedaOrigenId == monedaOrigenId && p.MonedaDestinoId == monedaDestinoId)
                .Select(p => (int?)p.ParMonedaId)
                .FirstOrDefaultAsync();
        }

        public async Task<(string OrigenIso, string DestinoIso)?> ObtenerIsosPorParIdAsync(int parMonedaId)
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .Where(p => p.ParMonedaId == parMonedaId)
                .FirstOrDefaultAsync();

            if (par == null)
                return null;

            return (par.MonedaOrigen.CodigoIso, par.MonedaDestino.CodigoIso);
        }
    }
}
