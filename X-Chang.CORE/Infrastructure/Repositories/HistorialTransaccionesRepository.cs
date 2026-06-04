using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.HistorialTransacciones;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class HistorialTransaccionesRepository : IHistorialTransaccionesRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public HistorialTransaccionesRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<PaginadoDto<OrdenCompraHistorialDto>> ObtenerOrdenesCompraAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.OrdenesCompra
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Where(o => o.UsuarioId == usuarioId)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(o => o.FechaCreacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(o => o.FechaCreacion <= fechaHasta.Value);

            query = query.OrderByDescending(o => o.FechaCreacion);

            var total = await query.CountAsync();
            var items = await AplicarPaginacion(query, pagina, registrosPorPagina);

            var lista = items.Select(o => new OrdenCompraHistorialDto
            {
                OrdenCompraId = o.OrdenCompraId,
                FechaHora = o.FechaCreacion,
                ParMonedas = $"{o.ParMoneda.MonedaOrigen.CodigoIso}/{o.ParMoneda.MonedaDestino.CodigoIso}",
                CantidadOriginal = o.CantidadOriginal,
                CantidadObtenida = o.CantidadObtenida,
                CantidadPendiente = o.CantidadPendiente,
                PrecioUnitario = o.PrecioUnitario,
                TotalComprometido = o.TotalComprometido,
                TotalEjecutado = o.TotalEjecutado,
                Estado = o.Estado
            }).ToList();

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<OfertaVentaHistorialDto>> ObtenerOfertasVentaAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.OfertasVenta
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Where(o => o.UsuarioId == usuarioId)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(o => o.FechaCreacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(o => o.FechaCreacion <= fechaHasta.Value);

            query = query.OrderByDescending(o => o.FechaCreacion);

            var total = await query.CountAsync();
            var items = await AplicarPaginacion(query, pagina, registrosPorPagina);

            var lista = items.Select(o => new OfertaVentaHistorialDto
            {
                OfertaVentaId = o.OfertaVentaId,
                FechaHora = o.FechaCreacion,
                ParMonedas = $"{o.ParMoneda.MonedaOrigen.CodigoIso}/{o.ParMoneda.MonedaDestino.CodigoIso}",
                CantidadOriginal = o.CantidadOriginal,
                CantidadVendida = o.CantidadVendida,
                CantidadPendiente = o.CantidadPendiente,
                PrecioUnitario = o.PrecioUnitario,
                TotalEsperado = o.TotalEsperado,
                TotalRecibido = o.TotalRecibido,
                Estado = o.Estado
            }).ToList();

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<CompraInmediataHistorialDto>> ObtenerComprasInmediatasAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.OperacionesInmediatas
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Include(o => o.InverseOperacionPadre)
                    .ThenInclude(h => h.ParMoneda)
                        .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.InverseOperacionPadre)
                    .ThenInclude(h => h.ParMoneda)
                        .ThenInclude(p => p.MonedaDestino)
                .Where(o =>
                    o.UsuarioId == usuarioId &&
                    o.TipoOperacion == "Compra inmediata" &&
                    o.OperacionPadreId == null)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(o => o.FechaOperacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(o => o.FechaOperacion <= fechaHasta.Value);

            query = query.OrderByDescending(o => o.FechaOperacion);

            var total = await query.CountAsync();
            var items = await AplicarPaginacion(query, pagina, registrosPorPagina);

            var lista = items.Select(MapearCompraInmediataDto).ToList();

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<VentaInmediataHistorialDto>> ObtenerVentasInmediatasAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.OperacionesInmediatas
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Include(o => o.InverseOperacionPadre)
                    .ThenInclude(h => h.ParMoneda)
                        .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.InverseOperacionPadre)
                    .ThenInclude(h => h.ParMoneda)
                        .ThenInclude(p => p.MonedaDestino)
                .Where(o =>
                    o.UsuarioId == usuarioId &&
                    o.TipoOperacion == "Venta inmediata" &&
                    o.OperacionPadreId == null)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(o => o.FechaOperacion >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(o => o.FechaOperacion <= fechaHasta.Value);

            query = query.OrderByDescending(o => o.FechaOperacion);

            var total = await query.CountAsync();
            var items = await AplicarPaginacion(query, pagina, registrosPorPagina);

            var lista = items.Select(MapearVentaInmediataDto).ToList();

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<DepositoHistorialDto>> ObtenerDepositosAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.Depositos
                .Include(d => d.Moneda)
                .Include(d => d.MetodoPago)
                .Where(d => d.UsuarioId == usuarioId)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(d => d.FechaDeposito >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(d => d.FechaDeposito <= fechaHasta.Value);

            query = query.OrderByDescending(d => d.FechaDeposito);

            var total = await query.CountAsync();
            var items = await AplicarPaginacion(query, pagina, registrosPorPagina);

            var lista = items.Select(d => new DepositoHistorialDto
            {
                DepositoId = d.DepositoId,
                FechaHora = d.FechaDeposito,
                Moneda = d.Moneda.CodigoIso,
                MontoDepositado = d.MontoDepositado,
                MetodoPago = d.MetodoPago.Nombre,
                ComisionAplicada = d.ComisionAplicada,
                TotalPagado = d.TotalPagado,
                Estado = d.Estado
            }).ToList();

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<RetiroHistorialDto>> ObtenerRetirosAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.Retiros
                .Include(r => r.Moneda)
                .Include(r => r.MetodoPago)
                .Where(r => r.UsuarioId == usuarioId)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(r => r.FechaRetiro >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(r => r.FechaRetiro <= fechaHasta.Value);

            query = query.OrderByDescending(r => r.FechaRetiro);

            var total = await query.CountAsync();
            var items = await AplicarPaginacion(query, pagina, registrosPorPagina);

            var lista = items.Select(r => new RetiroHistorialDto
            {
                RetiroId = r.RetiroId,
                FechaHora = r.FechaRetiro,
                Moneda = r.Moneda.CodigoIso,
                MontoRetirado = r.MontoRetirado,
                MetodoCobro = r.MetodoPago.Nombre,
                ComisionAplicada = r.ComisionAplicada,
                MontoFinalRecibido = r.MontoFinalRecibido,
                Estado = r.Estado
            }).ToList();

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<HistorialTransaccionesResponseDto> ObtenerHistorialCompletoAsync(int usuarioId)
        {
            return new HistorialTransaccionesResponseDto
            {
                OrdenesCompra = await ObtenerOrdenesCompraAsync(usuarioId, null, null, 1, 0),
                OfertasVenta = await ObtenerOfertasVentaAsync(usuarioId, null, null, 1, 0),
                ComprasInmediatas = await ObtenerComprasInmediatasAsync(usuarioId, null, null, 1, 0),
                VentasInmediatas = await ObtenerVentasInmediatasAsync(usuarioId, null, null, 1, 0),
                Depositos = await ObtenerDepositosAsync(usuarioId, null, null, 1, 0),
                Retiros = await ObtenerRetirosAsync(usuarioId, null, null, 1, 0)
            };
        }

        private static CompraInmediataHistorialDto MapearCompraInmediataDto(OperacionesInmediatas o)
        {
            var hijos = o.InverseOperacionPadre
                .OrderBy(h => h.OperacionInmediataId)
                .ToList();

            return new CompraInmediataHistorialDto
            {
                OperacionInmediataId = o.OperacionInmediataId,
                FechaHora = o.FechaOperacion,
                ParMonedas = $"{o.ParMoneda.MonedaOrigen.CodigoIso}/{o.ParMoneda.MonedaDestino.CodigoIso}",
                CantidadObtenida = o.CantidadEjecutada,
                PrecioMinCompra = o.PrecioMinimo,
                PrecioMaxCompra = o.PrecioMaximo,
                PrecioPromedioCompra = o.PrecioPromedio,
                TotalPagado = o.TotalPagado ?? 0,
                Estado = o.Estado,
                MetodoEjecucion = o.MetodoEjecucion,
                TieneSaltos = hijos.Count > 0,
                SaltosRuta = hijos.Select(MapearCompraInmediataDto).ToList()
            };
        }

        private static VentaInmediataHistorialDto MapearVentaInmediataDto(OperacionesInmediatas o)
        {
            var hijos = o.InverseOperacionPadre
                .OrderBy(h => h.OperacionInmediataId)
                .ToList();

            return new VentaInmediataHistorialDto
            {
                OperacionInmediataId = o.OperacionInmediataId,
                FechaHora = o.FechaOperacion,
                ParMonedas = $"{o.ParMoneda.MonedaOrigen.CodigoIso}/{o.ParMoneda.MonedaDestino.CodigoIso}",
                CantidadVendida = o.CantidadEjecutada,
                PrecioMinVenta = o.PrecioMinimo,
                PrecioMaxVenta = o.PrecioMaximo,
                PrecioPromedioVenta = o.PrecioPromedio,
                TotalRecibido = o.TotalRecibido ?? 0,
                Estado = o.Estado,
                MetodoEjecucion = o.MetodoEjecucion,
                TieneSaltos = hijos.Count > 0,
                SaltosRuta = hijos.Select(MapearVentaInmediataDto).ToList()
            };
        }

        private static async Task<List<T>> AplicarPaginacion<T>(
            IQueryable<T> query, int pagina, int registrosPorPagina) where T : class
        {
            if (registrosPorPagina == 0)
                return await query.ToListAsync();

            return await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();
        }

        private static PaginadoDto<T> ConstruirPaginado<T>(
            List<T> lista, int total, int pagina, int registrosPorPagina)
        {
            return new PaginadoDto<T>
            {
                TotalRegistros = total,
                NumeroPagina = registrosPorPagina == 0 ? 1 : pagina,
                RegistrosPorPagina = registrosPorPagina,
                TotalPaginas = registrosPorPagina == 0 ? 1 : (int)Math.Ceiling((double)total / registrosPorPagina),
                Lista = lista
            };
        }
    }
}
