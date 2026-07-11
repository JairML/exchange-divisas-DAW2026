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
            var ordenes = await _context.OrdenesCompra
                .AsNoTracking()
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Include(o => o.EjecucionesOrden)
                .Include(o => o.CancelacionesOrdenOferta)
                .Where(o => o.UsuarioId == usuarioId)
                .ToListAsync();

            var ordenIds = ordenes.Select(o => o.OrdenCompraId).ToList();
            var movimientosPorOrden = await ObtenerMovimientosOrdenesAsync(usuarioId, ordenIds);

            var listaCompleta = ordenes
                .SelectMany(o => MapearEstadosOrdenCompra(
                    o,
                    movimientosPorOrden.TryGetValue(o.OrdenCompraId, out var movimientos)
                        ? movimientos
                        : new List<MovimientosBilletera>()))
                .Where(x => !fechaDesde.HasValue || x.FechaHora >= fechaDesde.Value)
                .Where(x => !fechaHasta.HasValue || x.FechaHora <= fechaHasta.Value)
                .OrderByDescending(x => x.FechaHora)
                .ThenByDescending(x => x.OrdenCompraId)
                .ToList();

            var total = listaCompleta.Count;
            var lista = AplicarPaginacionLista(listaCompleta, pagina, registrosPorPagina);

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<OfertaVentaHistorialDto>> ObtenerOfertasVentaAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var ofertas = await _context.OfertasVenta
                .AsNoTracking()
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .Include(o => o.EjecucionesOrden)
                .Include(o => o.CancelacionesOrdenOferta)
                .Where(o => o.UsuarioId == usuarioId)
                .ToListAsync();

            var ofertaIds = ofertas.Select(o => o.OfertaVentaId).ToList();
            var movimientosPorOferta = await ObtenerMovimientosOfertasAsync(usuarioId, ofertaIds);

            var listaCompleta = ofertas
                .SelectMany(o => MapearEstadosOfertaVenta(
                    o,
                    movimientosPorOferta.TryGetValue(o.OfertaVentaId, out var movimientos)
                        ? movimientos
                        : new List<MovimientosBilletera>()))
                .Where(x => !fechaDesde.HasValue || x.FechaHora >= fechaDesde.Value)
                .Where(x => !fechaHasta.HasValue || x.FechaHora <= fechaHasta.Value)
                .OrderByDescending(x => x.FechaHora)
                .ThenByDescending(x => x.OfertaVentaId)
                .ToList();

            var total = listaCompleta.Count;
            var lista = AplicarPaginacionLista(listaCompleta, pagina, registrosPorPagina);

            return ConstruirPaginado(lista, total, pagina, registrosPorPagina);
        }

        public async Task<PaginadoDto<CompraInmediataHistorialDto>> ObtenerComprasInmediatasAsync(
            int usuarioId, DateTime? fechaDesde, DateTime? fechaHasta, int pagina, int registrosPorPagina)
        {
            var query = _context.OperacionesInmediatas
                .AsNoTracking()
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
                .AsNoTracking()
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
                .AsNoTracking()
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
                .AsNoTracking()
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

        private async Task<Dictionary<int, List<MovimientosBilletera>>> ObtenerMovimientosOrdenesAsync(
            int usuarioId, List<int> ordenIds)
        {
            if (ordenIds.Count == 0)
                return new Dictionary<int, List<MovimientosBilletera>>();

            var movimientos = await _context.MovimientosBilletera
                .AsNoTracking()
                .Where(m =>
                    m.UsuarioId == usuarioId &&
                    m.ReferenciaId.HasValue &&
                    ordenIds.Contains(m.ReferenciaId.Value) &&
                    m.ReferenciaTipo != null &&
                    m.ReferenciaTipo.ToLower() == "ordenescompra" &&
                    m.Monto > 0)
                .OrderBy(m => m.FechaMovimiento)
                .ThenBy(m => m.MovimientoId)
                .ToListAsync();

            return movimientos
                .GroupBy(m => m.ReferenciaId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private async Task<Dictionary<int, List<MovimientosBilletera>>> ObtenerMovimientosOfertasAsync(
            int usuarioId, List<int> ofertaIds)
        {
            if (ofertaIds.Count == 0)
                return new Dictionary<int, List<MovimientosBilletera>>();

            var movimientos = await _context.MovimientosBilletera
                .AsNoTracking()
                .Where(m =>
                    m.UsuarioId == usuarioId &&
                    m.ReferenciaId.HasValue &&
                    ofertaIds.Contains(m.ReferenciaId.Value) &&
                    m.ReferenciaTipo != null &&
                    m.ReferenciaTipo.ToLower() == "ofertasventa" &&
                    m.Monto > 0)
                .OrderBy(m => m.FechaMovimiento)
                .ThenBy(m => m.MovimientoId)
                .ToListAsync();

            return movimientos
                .GroupBy(m => m.ReferenciaId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static IEnumerable<OrdenCompraHistorialDto> MapearEstadosOrdenCompra(
            OrdenesCompra orden, List<MovimientosBilletera> movimientosOrden)
        {
            var par = $"{orden.ParMoneda.MonedaOrigen.CodigoIso}/{orden.ParMoneda.MonedaDestino.CodigoIso}";
            var registros = new List<OrdenCompraHistorialDto>();
            decimal cantidadObtenida = 0;
            decimal totalEjecutado = 0;

            registros.Add(new OrdenCompraHistorialDto
            {
                OrdenCompraId = orden.OrdenCompraId,
                FechaHora = orden.FechaCreacion,
                ParMonedas = par,
                CantidadOriginal = orden.CantidadOriginal,
                CantidadObtenida = 0,
                CantidadPendiente = orden.CantidadOriginal,
                PrecioUnitario = orden.PrecioUnitario,
                TotalComprometido = orden.TotalComprometido,
                TotalEjecutado = 0,
                Estado = "Activa"
            });

            var eventosEjecucion = orden.EjecucionesOrden
                .Select(e => new EjecucionHistorialFuente
                {
                    Id = e.EjecucionId,
                    Fecha = e.FechaEjecucion,
                    Cantidad = e.CantidadEjecutada,
                    Total = e.TotalOperacion,
                    Fuente = "EjecucionOrden"
                })
                .Concat(movimientosOrden.Select(m => new EjecucionHistorialFuente
                {
                    Id = m.MovimientoId,
                    Fecha = m.FechaMovimiento,
                    Cantidad = Math.Abs(m.Monto),
                    Total = Math.Abs(m.Monto) * orden.PrecioUnitario,
                    Fuente = "MovimientoBilletera"
                }))
                .Where(e => e.Cantidad > 0)
                .OrderBy(e => e.Fecha)
                .ThenBy(e => e.Id)
                .ToList();

            foreach (var ejecucion in eventosEjecucion)
            {
                cantidadObtenida += ejecucion.Cantidad;
                totalEjecutado += ejecucion.Total;
                var cantidadPendiente = Math.Max(0, orden.CantidadOriginal - cantidadObtenida);

                registros.Add(new OrdenCompraHistorialDto
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    FechaHora = ejecucion.Fecha,
                    ParMonedas = par,
                    CantidadOriginal = orden.CantidadOriginal,
                    CantidadObtenida = cantidadObtenida,
                    CantidadPendiente = cantidadPendiente,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalComprometido = orden.TotalComprometido,
                    TotalEjecutado = totalEjecutado,
                    Estado = cantidadPendiente <= 0.00000001m ? "Completada" : "Parcialmente ejecutada"
                });
            }

            foreach (var cancelacion in orden.CancelacionesOrdenOferta.OrderBy(c => c.FechaCancelacion).ThenBy(c => c.CancelacionId))
            {
                var cantidadEjecutada = cancelacion.CantidadEjecutada > 0 ? cancelacion.CantidadEjecutada : cantidadObtenida;
                var cantidadCancelada = cancelacion.CantidadCancelada > 0
                    ? cancelacion.CantidadCancelada
                    : Math.Max(0, orden.CantidadOriginal - cantidadEjecutada);

                registros.Add(new OrdenCompraHistorialDto
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    FechaHora = cancelacion.FechaCancelacion,
                    ParMonedas = par,
                    CantidadOriginal = orden.CantidadOriginal,
                    CantidadObtenida = cantidadEjecutada,
                    CantidadPendiente = cantidadCancelada,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalComprometido = orden.TotalComprometido,
                    TotalEjecutado = totalEjecutado,
                    Estado = "Cancelada"
                });
            }

            return registros;
        }

        private static IEnumerable<OfertaVentaHistorialDto> MapearEstadosOfertaVenta(
            OfertasVenta oferta, List<MovimientosBilletera> movimientosOferta)
        {
            var par = $"{oferta.ParMoneda.MonedaOrigen.CodigoIso}/{oferta.ParMoneda.MonedaDestino.CodigoIso}";
            var registros = new List<OfertaVentaHistorialDto>();
            decimal cantidadVendida = 0;
            decimal totalRecibido = 0;

            registros.Add(new OfertaVentaHistorialDto
            {
                OfertaVentaId = oferta.OfertaVentaId,
                FechaHora = oferta.FechaCreacion,
                ParMonedas = par,
                CantidadOriginal = oferta.CantidadOriginal,
                CantidadVendida = 0,
                CantidadPendiente = oferta.CantidadOriginal,
                PrecioUnitario = oferta.PrecioUnitario,
                TotalEsperado = oferta.TotalEsperado,
                TotalRecibido = 0,
                Estado = "Activa"
            });

            var eventosEjecucion = oferta.EjecucionesOrden
                .Select(e => new EjecucionHistorialFuente
                {
                    Id = e.EjecucionId,
                    Fecha = e.FechaEjecucion,
                    Cantidad = e.CantidadEjecutada,
                    Total = e.TotalOperacion,
                    Fuente = "EjecucionOrden"
                })
                .Concat(movimientosOferta.Select(m => new EjecucionHistorialFuente
                {
                    Id = m.MovimientoId,
                    Fecha = m.FechaMovimiento,
                    Cantidad = oferta.PrecioUnitario > 0 ? Math.Abs(m.Monto) / oferta.PrecioUnitario : 0,
                    Total = Math.Abs(m.Monto),
                    Fuente = "MovimientoBilletera"
                }))
                .Where(e => e.Cantidad > 0)
                .OrderBy(e => e.Fecha)
                .ThenBy(e => e.Id)
                .ToList();

            foreach (var ejecucion in eventosEjecucion)
            {
                cantidadVendida += ejecucion.Cantidad;
                totalRecibido += ejecucion.Total;
                var cantidadPendiente = Math.Max(0, oferta.CantidadOriginal - cantidadVendida);

                registros.Add(new OfertaVentaHistorialDto
                {
                    OfertaVentaId = oferta.OfertaVentaId,
                    FechaHora = ejecucion.Fecha,
                    ParMonedas = par,
                    CantidadOriginal = oferta.CantidadOriginal,
                    CantidadVendida = cantidadVendida,
                    CantidadPendiente = cantidadPendiente,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalEsperado = oferta.TotalEsperado,
                    TotalRecibido = totalRecibido,
                    Estado = cantidadPendiente <= 0.00000001m ? "Completada" : "Parcialmente ejecutada"
                });
            }

            foreach (var cancelacion in oferta.CancelacionesOrdenOferta.OrderBy(c => c.FechaCancelacion).ThenBy(c => c.CancelacionId))
            {
                var cantidadEjecutada = cancelacion.CantidadEjecutada > 0 ? cancelacion.CantidadEjecutada : cantidadVendida;
                var cantidadCancelada = cancelacion.CantidadCancelada > 0
                    ? cancelacion.CantidadCancelada
                    : Math.Max(0, oferta.CantidadOriginal - cantidadEjecutada);

                registros.Add(new OfertaVentaHistorialDto
                {
                    OfertaVentaId = oferta.OfertaVentaId,
                    FechaHora = cancelacion.FechaCancelacion,
                    ParMonedas = par,
                    CantidadOriginal = oferta.CantidadOriginal,
                    CantidadVendida = cantidadEjecutada,
                    CantidadPendiente = cantidadCancelada,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalEsperado = oferta.TotalEsperado,
                    TotalRecibido = totalRecibido,
                    Estado = "Cancelada"
                });
            }

            return registros;
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

        private sealed class EjecucionHistorialFuente
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public decimal Cantidad { get; set; }
            public decimal Total { get; set; }
            public string Fuente { get; set; } = string.Empty;
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

        private static List<T> AplicarPaginacionLista<T>(List<T> lista, int pagina, int registrosPorPagina)
        {
            if (registrosPorPagina == 0)
                return lista;

            return lista
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();
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
