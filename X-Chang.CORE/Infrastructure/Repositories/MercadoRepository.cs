using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class MercadoRepository : IMercadoRepository
    {
        private readonly ExchangeDivisasDbContext _context;
        private static readonly string[] EstadosActivos = { "Activa", "Parcialmente ejecutada" };

        public MercadoRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EsAdministradorActivoAsync(int usuarioId)
        {
            return await _context.Usuarios.AnyAsync(u =>
                u.UsuarioId == usuarioId &&
                u.Estado == "Activo" &&
                u.Rol.Nombre == "Administrador");
        }

        public async Task<OperacionesActivasResponseDto> ObtenerOperacionesActivasAsync(int usuarioId, FiltroOperacionesActivasDto filtro)
        {
            var ordenesQuery = _context.OrdenesCompra
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
                .Where(o => o.UsuarioId == usuarioId && EstadosActivos.Contains(o.Estado));

            var ofertasQuery = _context.OfertasVenta
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
                .Where(o => o.UsuarioId == usuarioId && EstadosActivos.Contains(o.Estado));

            if (filtro.FechaDesde.HasValue)
            {
                var desde = filtro.FechaDesde.Value.Date;
                ordenesQuery = ordenesQuery.Where(o => o.FechaCreacion >= desde);
                ofertasQuery = ofertasQuery.Where(o => o.FechaCreacion >= desde);
            }

            if (filtro.FechaHasta.HasValue)
            {
                var hasta = filtro.FechaHasta.Value.Date.AddDays(1);
                ordenesQuery = ordenesQuery.Where(o => o.FechaCreacion < hasta);
                ofertasQuery = ofertasQuery.Where(o => o.FechaCreacion < hasta);
            }

            var ordenesTotal = await ordenesQuery.CountAsync();
            var ofertasTotal = await ofertasQuery.CountAsync();
            var totalRegistros = ordenesTotal + ofertasTotal;
            var registrosPorPagina = int.Parse(filtro.RegistrosPorPagina);
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (decimal)registrosPorPagina));

            if (filtro.Pagina > totalPaginas) filtro.Pagina = totalPaginas;
            if (filtro.Pagina < 1) filtro.Pagina = 1;

            // Se pagina cada sección por separado para que el frontend pueda mostrar ambas columnas.
            var ordenes = await ordenesQuery
                .OrderByDescending(o => o.FechaCreacion)
                .Skip((filtro.Pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(o => new OperacionActivaDto
                {
                    Id = o.OrdenCompraId,
                    TipoOperacion = "Orden de compra",
                    FechaCreacion = o.FechaCreacion,
                    ParMonedaId = o.ParMonedaId,
                    Par = o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
                    PrecioUnitario = o.PrecioUnitario,
                    CantidadOriginal = o.CantidadOriginal,
                    CantidadEjecutada = o.CantidadObtenida,
                    CantidadRestante = o.CantidadPendiente,
                    TotalOriginal = o.TotalComprometido,
                    TotalEjecutado = o.TotalEjecutado,
                    TotalRestante = o.TotalComprometido - o.TotalEjecutado,
                    Estado = o.Estado,
                    PuedeCancelar = EstadosActivos.Contains(o.Estado)
                })
                .ToListAsync();

            var ofertas = await ofertasQuery
                .OrderByDescending(o => o.FechaCreacion)
                .Skip((filtro.Pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(o => new OperacionActivaDto
                {
                    Id = o.OfertaVentaId,
                    TipoOperacion = "Oferta de venta",
                    FechaCreacion = o.FechaCreacion,
                    ParMonedaId = o.ParMonedaId,
                    Par = o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
                    PrecioUnitario = o.PrecioUnitario,
                    CantidadOriginal = o.CantidadOriginal,
                    CantidadEjecutada = o.CantidadVendida,
                    CantidadRestante = o.CantidadPendiente,
                    TotalOriginal = o.TotalEsperado,
                    TotalEjecutado = o.TotalRecibido,
                    TotalRestante = o.CantidadPendiente * o.PrecioUnitario,
                    Estado = o.Estado,
                    PuedeCancelar = EstadosActivos.Contains(o.Estado)
                })
                .ToListAsync();

            return new OperacionesActivasResponseDto
            {
                OrdenesCompra = ordenes,
                OfertasVenta = ofertas,
                PaginaActual = filtro.Pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TienePaginaAnterior = filtro.Pagina > 1,
                TienePaginaSiguiente = filtro.Pagina < totalPaginas,
                Mensaje = totalRegistros == 0 ? "No existen órdenes ni ofertas activas" : string.Empty
            };
        }

        public async Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId, bool verTodasOrdenes, bool verTodasOfertas)
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId && p.Activo);

            if (par == null)
                throw new ArgumentException("El par de monedas no existe.");

            var ordenesQuery = _context.OrdenesCompra
                .Where(o => o.ParMonedaId == parMonedaId && o.CantidadPendiente > 0 && EstadosActivos.Contains(o.Estado))
                .OrderByDescending(o => o.PrecioUnitario)
                .ThenBy(o => o.FechaCreacion)
                .Select(o => new LibroOrdenesRegistroDto
                {
                    Id = o.OrdenCompraId,
                    FechaCreacion = o.FechaCreacion,
                    Cantidad = o.CantidadPendiente,
                    PrecioUnitario = o.PrecioUnitario,
                    Total = o.CantidadPendiente * o.PrecioUnitario,
                    Estado = o.Estado
                });

            var ofertasQuery = _context.OfertasVenta
                .Where(o => o.ParMonedaId == parMonedaId && o.CantidadPendiente > 0 && EstadosActivos.Contains(o.Estado))
                .OrderBy(o => o.PrecioUnitario)
                .ThenBy(o => o.FechaCreacion)
                .Select(o => new LibroOrdenesRegistroDto
                {
                    Id = o.OfertaVentaId,
                    FechaCreacion = o.FechaCreacion,
                    Cantidad = o.CantidadPendiente,
                    PrecioUnitario = o.PrecioUnitario,
                    Total = o.CantidadPendiente * o.PrecioUnitario,
                    Estado = o.Estado
                });

            var ordenes = await (verTodasOrdenes ? ordenesQuery : ordenesQuery.Take(10)).ToListAsync();
            var ofertas = await (verTodasOfertas ? ofertasQuery : ofertasQuery.Take(10)).ToListAsync();

            return new LibroOrdenesDto
            {
                ParMonedaId = parMonedaId,
                Par = par.MonedaOrigen.CodigoIso + "/" + par.MonedaDestino.CodigoIso,
                OrdenesCompra = ordenes,
                OfertasVenta = ofertas,
                MensajeOrdenes = ordenes.Any() ? string.Empty : "No existen órdenes de compra activas",
                MensajeOfertas = ofertas.Any() ? string.Empty : "No existen ofertas de venta activas"
            };
        }

        public async Task<ResumenOrdenCompraDto> ObtenerResumenOrdenCompraAsync(int usuarioId, CrearOrdenCompraRequestDto request)
        {
            var par = await ObtenerParAsync(request.ParMonedaId);
            var totalComprometido = request.CantidadAObtener * request.PrecioUnitario;
            var saldoDisponible = await ObtenerSaldoDisponibleAsync(usuarioId, par.MonedaOrigenId);
            var ofertas = await ObtenerOfertasCompatiblesAsync(request.ParMonedaId, request.PrecioUnitario, usuarioId);

            decimal pendiente = request.CantidadAObtener;
            decimal ejecutable = 0;
            decimal totalEjecutado = 0;
            var precios = new List<decimal>();

            foreach (var oferta in ofertas)
            {
                if (pendiente <= 0) break;
                var cantidad = Math.Min(pendiente, oferta.CantidadPendiente);
                var total = cantidad * oferta.PrecioUnitario;
                ejecutable += cantidad;
                totalEjecutado += total;
                pendiente -= cantidad;
                precios.Add(oferta.PrecioUnitario);
            }

            return new ResumenOrdenCompraDto
            {
                ParMonedaId = par.ParMonedaId,
                Par = par.MonedaOrigen.CodigoIso + "/" + par.MonedaDestino.CodigoIso,
                CantidadAObtener = request.CantidadAObtener,
                PrecioUnitario = request.PrecioUnitario,
                TotalComprometido = totalComprometido,
                SaldoDisponible = saldoDisponible,
                SaldoSuficiente = saldoDisponible >= totalComprometido,
                PuedeEjecutarseAutomaticamente = ejecutable > 0,
                CantidadEjecutableInmediata = ejecutable,
                CantidadPendienteEstimada = request.CantidadAObtener - ejecutable,
                PrecioMinimoCompra = precios.Any() ? precios.Min() : null,
                PrecioMaximoCompra = precios.Any() ? precios.Max() : null,
                PrecioPromedioCompra = ejecutable > 0 ? totalEjecutado / ejecutable : null,
                TotalEstimadoEjecutado = totalEjecutado,
                Mensaje = saldoDisponible < totalComprometido ? "Saldo insuficiente" : "Orden de compra válida"
            };
        }

        public async Task<OrdenCompraResultadoDto> CrearOrdenCompraAsync(int usuarioId, CrearOrdenCompraRequestDto request)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            var ahora = DateTime.Now;
            var par = await ObtenerParAsync(request.ParMonedaId);
            var totalMaximo = request.CantidadAObtener * request.PrecioUnitario;

            var saldoOrigen = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaOrigenId);
            if (saldoOrigen.SaldoDisponible < totalMaximo)
                throw new InvalidOperationException("Saldo insuficiente");

            var saldoOrigenAnterior = saldoOrigen.SaldoDisponible;
            saldoOrigen.SaldoDisponible -= totalMaximo;
            saldoOrigen.FechaActualizacion = ahora;

            var orden = new OrdenesCompra
            {
                UsuarioId = usuarioId,
                ParMonedaId = par.ParMonedaId,
                CantidadOriginal = request.CantidadAObtener,
                CantidadObtenida = 0,
                CantidadPendiente = request.CantidadAObtener,
                PrecioUnitario = request.PrecioUnitario,
                TotalComprometido = totalMaximo,
                TotalEjecutado = 0,
                Estado = "Activa",
                FechaCreacion = ahora,
                FechaActualizacion = ahora
            };

            _context.OrdenesCompra.Add(orden);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaOrigenId,
                TipoMovimiento = "OrdenCompra",
                Monto = -totalMaximo,
                SaldoAnterior = saldoOrigenAnterior,
                SaldoPosterior = saldoOrigen.SaldoDisponible,
                FechaMovimiento = ahora,
                ReferenciaTipo = "OrdenCompra",
                ReferenciaId = orden.OrdenCompraId
            });

            var ejecuciones = new List<EjecucionMercadoDto>();
            var ofertas = await ObtenerOfertasCompatiblesAsync(par.ParMonedaId, request.PrecioUnitario, usuarioId);

            foreach (var oferta in ofertas)
            {
                if (orden.CantidadPendiente <= 0) break;
                var cantidad = Math.Min(orden.CantidadPendiente, oferta.CantidadPendiente);
                var total = cantidad * oferta.PrecioUnitario;

                orden.CantidadObtenida += cantidad;
                orden.CantidadPendiente -= cantidad;
                orden.TotalEjecutado += total;
                orden.FechaActualizacion = ahora;

                oferta.CantidadVendida += cantidad;
                oferta.CantidadPendiente -= cantidad;
                oferta.TotalRecibido += total;
                oferta.FechaActualizacion = ahora;
                oferta.Estado = oferta.CantidadPendiente <= 0 ? "Completada" : "Parcialmente ejecutada";

                await SincronizarOrdenEspejoExactaDesdeOfertaAsync(oferta);

                var saldoCompradorDestino = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaDestinoId);
                var saldoCompradorDestinoAnterior = saldoCompradorDestino.SaldoDisponible;
                saldoCompradorDestino.SaldoDisponible += cantidad;
                saldoCompradorDestino.FechaActualizacion = ahora;

                var saldoVendedorOrigen = await ObtenerSaldoEntityAsync(oferta.UsuarioId, par.MonedaOrigenId);
                var saldoVendedorOrigenAnterior = saldoVendedorOrigen.SaldoDisponible;
                saldoVendedorOrigen.SaldoDisponible += total;
                saldoVendedorOrigen.FechaActualizacion = ahora;

                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    ParMonedaId = par.ParMonedaId,
                    CompradorId = usuarioId,
                    VendedorId = oferta.UsuarioId,
                    CantidadEjecutada = cantidad,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalOperacion = total,
                    FechaEjecucion = ahora
                };

                _context.EjecucionesOrden.Add(ejecucion);
                await _context.SaveChangesAsync();

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = par.MonedaDestinoId,
                    TipoMovimiento = "OrdenCompra",
                    Monto = cantidad,
                    SaldoAnterior = saldoCompradorDestinoAnterior,
                    SaldoPosterior = saldoCompradorDestino.SaldoDisponible,
                    FechaMovimiento = ahora,
                    ReferenciaTipo = "EjecucionOrden",
                    ReferenciaId = ejecucion.EjecucionId
                });

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = oferta.UsuarioId,
                    MonedaId = par.MonedaOrigenId,
                    TipoMovimiento = "OfertaVenta",
                    Monto = total,
                    SaldoAnterior = saldoVendedorOrigenAnterior,
                    SaldoPosterior = saldoVendedorOrigen.SaldoDisponible,
                    FechaMovimiento = ahora,
                    ReferenciaTipo = "EjecucionOrden",
                    ReferenciaId = ejecucion.EjecucionId
                });

                await CrearNotificacionAsync(oferta.UsuarioId, "Oferta de venta", "Oferta ejecutada", $"Tu oferta recibió una ejecución por {cantidad}.", "EjecucionOrden", ejecucion.EjecucionId);

                ejecuciones.Add(new EjecucionMercadoDto
                {
                    EjecucionId = ejecucion.EjecucionId,
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    CompradorId = usuarioId,
                    VendedorId = oferta.UsuarioId,
                    CantidadEjecutada = cantidad,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalOperacion = total,
                    FechaEjecucion = ahora
                });
            }

            orden.Estado = orden.CantidadPendiente <= 0 ? "Completada" : orden.CantidadObtenida > 0 ? "Parcialmente ejecutada" : "Activa";
            var montoReembolsado = await AjustarReservaOrdenYReembolsarMejorPrecioAsync(orden, par.MonedaOrigenId, ahora);
            var ofertaEspejoId = await CrearOfertaEspejoSiOrdenQuedoPendienteAsync(orden, par, ahora);

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Orden de compra",
                ReferenciaId = orden.OrdenCompraId,
                ParMonedaId = orden.ParMonedaId,
                Estado = orden.Estado,
                MetodoEjecucion = ejecuciones.Any() ? "Automática" : "Libro de órdenes",
                FechaHora = ahora
            });

            await CrearNotificacionAsync(usuarioId, "Orden de compra", "Orden de compra registrada", $"Tu orden de compra quedó en estado {orden.Estado}.", "OrdenCompra", orden.OrdenCompraId);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new OrdenCompraResultadoDto
            {
                OrdenCompraId = orden.OrdenCompraId,
                OfertaEspejoId = ofertaEspejoId,
                ParMonedaId = orden.ParMonedaId,
                Par = par.MonedaOrigen.CodigoIso + "/" + par.MonedaDestino.CodigoIso,
                CantidadOriginal = orden.CantidadOriginal,
                CantidadObtenida = orden.CantidadObtenida,
                CantidadPendiente = orden.CantidadPendiente,
                PrecioUnitario = orden.PrecioUnitario,
                TotalComprometido = orden.TotalComprometido,
                TotalEjecutado = orden.TotalEjecutado,
                MontoReembolsadoPorMejorPrecio = montoReembolsado,
                Estado = orden.Estado,
                FechaCreacion = orden.FechaCreacion,
                Ejecuciones = ejecuciones
            };
        }

        public async Task<ResumenOfertaVentaDto> ObtenerResumenOfertaVentaAsync(int usuarioId, CrearOfertaVentaRequestDto request)
        {
            var par = await ObtenerParAsync(request.ParMonedaId);
            var totalEsperado = request.CantidadAVender * request.PrecioUnitario;
            var saldoDisponible = await ObtenerSaldoDisponibleAsync(usuarioId, par.MonedaDestinoId);
            var ordenes = await ObtenerOrdenesCompatiblesAsync(request.ParMonedaId, request.PrecioUnitario, usuarioId);

            decimal pendiente = request.CantidadAVender;
            decimal ejecutable = 0;
            decimal totalRecibido = 0;
            var precios = new List<decimal>();

            foreach (var orden in ordenes)
            {
                if (pendiente <= 0) break;
                var cantidad = Math.Min(pendiente, orden.CantidadPendiente);
                var total = cantidad * orden.PrecioUnitario;
                ejecutable += cantidad;
                totalRecibido += total;
                pendiente -= cantidad;
                precios.Add(orden.PrecioUnitario);
            }

            return new ResumenOfertaVentaDto
            {
                ParMonedaId = par.ParMonedaId,
                Par = par.MonedaOrigen.CodigoIso + "/" + par.MonedaDestino.CodigoIso,
                CantidadAVender = request.CantidadAVender,
                PrecioUnitario = request.PrecioUnitario,
                TotalEsperado = totalEsperado,
                SaldoDisponible = saldoDisponible,
                SaldoSuficiente = saldoDisponible >= request.CantidadAVender,
                PuedeEjecutarseAutomaticamente = ejecutable > 0,
                CantidadEjecutableInmediata = ejecutable,
                CantidadPendienteEstimada = request.CantidadAVender - ejecutable,
                PrecioMinimoVenta = precios.Any() ? precios.Min() : null,
                PrecioMaximoVenta = precios.Any() ? precios.Max() : null,
                PrecioPromedioVenta = ejecutable > 0 ? totalRecibido / ejecutable : null,
                TotalEstimadoRecibido = totalRecibido,
                Mensaje = saldoDisponible < request.CantidadAVender ? "Saldo insuficiente" : "Oferta de venta válida"
            };
        }

        public async Task<OfertaVentaResultadoDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaVentaRequestDto request)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            var ahora = DateTime.Now;
            var par = await ObtenerParAsync(request.ParMonedaId);
            var totalEsperado = request.CantidadAVender * request.PrecioUnitario;

            var saldoDestino = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaDestinoId);
            if (saldoDestino.SaldoDisponible < request.CantidadAVender)
                throw new InvalidOperationException("Saldo insuficiente");

            var saldoDestinoAnterior = saldoDestino.SaldoDisponible;
            saldoDestino.SaldoDisponible -= request.CantidadAVender;
            saldoDestino.FechaActualizacion = ahora;

            var oferta = new OfertasVenta
            {
                UsuarioId = usuarioId,
                ParMonedaId = par.ParMonedaId,
                CantidadOriginal = request.CantidadAVender,
                CantidadVendida = 0,
                CantidadPendiente = request.CantidadAVender,
                PrecioUnitario = request.PrecioUnitario,
                TotalEsperado = totalEsperado,
                TotalRecibido = 0,
                Estado = "Activa",
                FechaCreacion = ahora,
                FechaActualizacion = ahora
            };

            _context.OfertasVenta.Add(oferta);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaDestinoId,
                TipoMovimiento = "OfertaVenta",
                Monto = -request.CantidadAVender,
                SaldoAnterior = saldoDestinoAnterior,
                SaldoPosterior = saldoDestino.SaldoDisponible,
                FechaMovimiento = ahora,
                ReferenciaTipo = "OfertaVenta",
                ReferenciaId = oferta.OfertaVentaId
            });

            var ejecuciones = new List<EjecucionMercadoDto>();
            var ordenes = await ObtenerOrdenesCompatiblesAsync(par.ParMonedaId, request.PrecioUnitario, usuarioId);

            foreach (var orden in ordenes)
            {
                if (oferta.CantidadPendiente <= 0) break;
                var cantidad = Math.Min(oferta.CantidadPendiente, orden.CantidadPendiente);
                var total = cantidad * orden.PrecioUnitario;

                oferta.CantidadVendida += cantidad;
                oferta.CantidadPendiente -= cantidad;
                oferta.TotalRecibido += total;
                oferta.FechaActualizacion = ahora;

                orden.CantidadObtenida += cantidad;
                orden.CantidadPendiente -= cantidad;
                orden.TotalEjecutado += total;
                orden.FechaActualizacion = ahora;
                orden.Estado = orden.CantidadPendiente <= 0 ? "Completada" : "Parcialmente ejecutada";

                await AjustarReservaOrdenYReembolsarMejorPrecioAsync(orden, par.MonedaOrigenId, ahora);
                await SincronizarOfertaEspejoExactaDesdeOrdenAsync(orden);

                var saldoVendedorOrigen = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaOrigenId);
                var saldoVendedorOrigenAnterior = saldoVendedorOrigen.SaldoDisponible;
                saldoVendedorOrigen.SaldoDisponible += total;
                saldoVendedorOrigen.FechaActualizacion = ahora;

                var saldoCompradorDestino = await ObtenerSaldoEntityAsync(orden.UsuarioId, par.MonedaDestinoId);
                var saldoCompradorDestinoAnterior = saldoCompradorDestino.SaldoDisponible;
                saldoCompradorDestino.SaldoDisponible += cantidad;
                saldoCompradorDestino.FechaActualizacion = ahora;

                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    ParMonedaId = par.ParMonedaId,
                    CompradorId = orden.UsuarioId,
                    VendedorId = usuarioId,
                    CantidadEjecutada = cantidad,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalOperacion = total,
                    FechaEjecucion = ahora
                };

                _context.EjecucionesOrden.Add(ejecucion);
                await _context.SaveChangesAsync();

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = par.MonedaOrigenId,
                    TipoMovimiento = "OfertaVenta",
                    Monto = total,
                    SaldoAnterior = saldoVendedorOrigenAnterior,
                    SaldoPosterior = saldoVendedorOrigen.SaldoDisponible,
                    FechaMovimiento = ahora,
                    ReferenciaTipo = "EjecucionOrden",
                    ReferenciaId = ejecucion.EjecucionId
                });

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = orden.UsuarioId,
                    MonedaId = par.MonedaDestinoId,
                    TipoMovimiento = "OrdenCompra",
                    Monto = cantidad,
                    SaldoAnterior = saldoCompradorDestinoAnterior,
                    SaldoPosterior = saldoCompradorDestino.SaldoDisponible,
                    FechaMovimiento = ahora,
                    ReferenciaTipo = "EjecucionOrden",
                    ReferenciaId = ejecucion.EjecucionId
                });

                await CrearNotificacionAsync(orden.UsuarioId, "Orden de compra", "Orden ejecutada", $"Tu orden recibió una ejecución por {cantidad}.", "EjecucionOrden", ejecucion.EjecucionId);

                ejecuciones.Add(new EjecucionMercadoDto
                {
                    EjecucionId = ejecucion.EjecucionId,
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    CompradorId = orden.UsuarioId,
                    VendedorId = usuarioId,
                    CantidadEjecutada = cantidad,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalOperacion = total,
                    FechaEjecucion = ahora
                });
            }

            oferta.Estado = oferta.CantidadPendiente <= 0 ? "Completada" : oferta.CantidadVendida > 0 ? "Parcialmente ejecutada" : "Activa";
            var ordenEspejoId = await CrearOrdenEspejoSiOfertaQuedoPendienteAsync(oferta, par, ahora);

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Oferta de venta",
                ReferenciaId = oferta.OfertaVentaId,
                ParMonedaId = oferta.ParMonedaId,
                Estado = oferta.Estado,
                MetodoEjecucion = ejecuciones.Any() ? "Automática" : "Libro de órdenes",
                FechaHora = ahora
            });

            await CrearNotificacionAsync(usuarioId, "Oferta de venta", "Oferta de venta registrada", $"Tu oferta de venta quedó en estado {oferta.Estado}.", "OfertaVenta", oferta.OfertaVentaId);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new OfertaVentaResultadoDto
            {
                OfertaVentaId = oferta.OfertaVentaId,
                OrdenCompraEspejoId = ordenEspejoId,
                ParMonedaId = oferta.ParMonedaId,
                Par = par.MonedaOrigen.CodigoIso + "/" + par.MonedaDestino.CodigoIso,
                CantidadOriginal = oferta.CantidadOriginal,
                CantidadVendida = oferta.CantidadVendida,
                CantidadPendiente = oferta.CantidadPendiente,
                PrecioUnitario = oferta.PrecioUnitario,
                TotalEsperado = oferta.TotalEsperado,
                TotalRecibido = oferta.TotalRecibido,
                Estado = oferta.Estado,
                FechaCreacion = oferta.FechaCreacion,
                Ejecuciones = ejecuciones
            };
        }

        public async Task<PanelAdministrativoDto> ObtenerPanelAdministrativoAsync(FiltroPanelAdministrativoDto filtro)
        {
            var desde = filtro.FechaDesde?.Date ?? DateTime.Today;
            var hastaExclusivo = filtro.FechaHasta?.Date.AddDays(1) ?? DateTime.Today.AddDays(1);

            var totalDepositos = await _context.Depositos
                .Where(d => d.FechaDeposito >= desde && d.FechaDeposito < hastaExclusivo && d.Estado == "Completada")
                .SumAsync(d => (decimal?)d.MontoDepositado) ?? 0m;

            var totalRetiros = await _context.Retiros
                .Where(r => r.FechaRetiro >= desde && r.FechaRetiro < hastaExclusivo && r.Estado == "Completada")
                .SumAsync(r => (decimal?)r.MontoRetirado) ?? 0m;

            var ejecuciones = _context.EjecucionesOrden
                .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion < hastaExclusivo);

            var volumenOperado = await ejecuciones.SumAsync(e => (decimal?)e.TotalOperacion) ?? 0m;
            var transaccionesEjecutadas = await ejecuciones.CountAsync();

            var volumenPorDia = await ejecuciones
                .GroupBy(e => e.FechaEjecucion.Date)
                .Select(g => new SerieOperacionesDiaDto
                {
                    Fecha = g.Key,
                    Volumen = g.Sum(e => e.TotalOperacion),
                    CantidadOperaciones = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            var volumenPorMoneda = await _context.EjecucionesOrden
                .Include(e => e.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Where(e => e.FechaEjecucion >= desde && e.FechaEjecucion < hastaExclusivo)
                .GroupBy(e => new { e.ParMoneda.MonedaOrigenId, e.ParMoneda.MonedaOrigen.CodigoIso })
                .Select(g => new VolumenPorMonedaDto
                {
                    MonedaId = g.Key.MonedaOrigenId,
                    CodigoMoneda = g.Key.CodigoIso,
                    Volumen = g.Sum(e => e.TotalOperacion)
                })
                .OrderByDescending(x => x.Volumen)
                .ToListAsync();

            var usuariosActivos = await _context.HistorialTransacciones
                .Where(h => h.FechaHora >= desde && h.FechaHora < hastaExclusivo)
                .Select(h => h.UsuarioId)
                .Distinct()
                .CountAsync();

            var distribucionPorTipo = await _context.HistorialTransacciones
                .Where(h => h.FechaHora >= desde && h.FechaHora < hastaExclusivo)
                .GroupBy(h => h.TipoOperacion)
                .Select(g => new DistribucionTipoOperacionDto
                {
                    TipoOperacion = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync();

            var monedas = await _context.Monedas
                .Where(m => m.Activa)
                .Select(m => new MonedaResumenAdminDto
                {
                    MonedaId = m.MonedaId,
                    CodigoMoneda = m.CodigoIso,
                    VolumenOperado = _context.EjecucionesOrden
                        .Where(e => e.ParMoneda.MonedaOrigenId == m.MonedaId
                            && e.FechaEjecucion >= desde && e.FechaEjecucion < hastaExclusivo)
                        .Sum(e => (decimal?)e.TotalOperacion) ?? 0m,
                    CantidadOperaciones = _context.EjecucionesOrden
                        .Count(e => (e.ParMoneda.MonedaOrigenId == m.MonedaId || e.ParMoneda.MonedaDestinoId == m.MonedaId)
                            && e.FechaEjecucion >= desde && e.FechaEjecucion < hastaExclusivo),
                    CantidadComprada = _context.EjecucionesOrden
                        .Where(e => e.ParMoneda.MonedaDestinoId == m.MonedaId
                            && e.FechaEjecucion >= desde && e.FechaEjecucion < hastaExclusivo)
                        .Sum(e => (decimal?)e.CantidadEjecutada) ?? 0m,
                    CantidadVendida = _context.EjecucionesOrden
                        .Where(e => e.ParMoneda.MonedaOrigenId == m.MonedaId
                            && e.FechaEjecucion >= desde && e.FechaEjecucion < hastaExclusivo)
                        .Sum(e => (decimal?)e.TotalOperacion) ?? 0m,
                    TotalDepositado = _context.Depositos
                        .Where(d => d.MonedaId == m.MonedaId && d.Estado == "Completada"
                            && d.FechaDeposito >= desde && d.FechaDeposito < hastaExclusivo)
                        .Sum(d => (decimal?)d.MontoDepositado) ?? 0m,
                    TotalRetirado = _context.Retiros
                        .Where(r => r.MonedaId == m.MonedaId && r.Estado == "Completada"
                            && r.FechaRetiro >= desde && r.FechaRetiro < hastaExclusivo)
                        .Sum(r => (decimal?)r.MontoRetirado) ?? 0m
                })
                .Where(x => x.VolumenOperado > 0 || x.CantidadOperaciones > 0 || x.TotalDepositado > 0 || x.TotalRetirado > 0)
                .OrderByDescending(x => x.VolumenOperado)
                .ToListAsync();

            var mejoresRutas = await _context.RutasConversion
                .Include(r => r.MonedaInicial)
                .Include(r => r.MonedaFinal)
                .Include(r => r.RutaConversionSaltos).ThenInclude(s => s.MonedaOrigen)
                .Include(r => r.RutaConversionSaltos).ThenInclude(s => s.MonedaDestino)
                .Where(r => r.FechaCreacion >= desde && r.FechaCreacion < hastaExclusivo)
                .OrderByDescending(r => r.FechaCreacion)
                .Take(50)
                .Select(r => new MejorRutaAdminDto
                {
                    FechaCreacion = r.FechaCreacion,
                    MonedaInicial = r.MonedaInicial.CodigoIso,
                    MonedaFinal = r.MonedaFinal.CodigoIso,
                    CantidadSaltos = r.CantidadSaltos,
                    AhorroEstimado = r.AhorroEstimado,
                    GananciaEstimada = r.GananciaEstimada,
                    Saltos = r.RutaConversionSaltos
                        .OrderBy(s => s.NumeroSalto)
                        .Select(s => new SaltoMejorRutaAdminDto
                        {
                            NumeroSalto = s.NumeroSalto,
                            Par = s.MonedaOrigen.CodigoIso + "/" + s.MonedaDestino.CodigoIso,
                            CantidadConvertida = s.CantidadConvertida,
                            PrecioMinimo = s.PrecioMinimo,
                            PrecioMaximo = s.PrecioMaximo,
                            PrecioPromedio = s.PrecioPromedio,
                            ResultadoObtenido = s.ResultadoObtenido
                        }).ToList()
                })
                .ToListAsync();

            return new PanelAdministrativoDto
            {
                TotalUsuariosRegistrados = await _context.Usuarios.CountAsync(),
                UsuariosActivosEnPeriodo = usuariosActivos,
                TotalDepositos = totalDepositos,
                TotalRetiros = totalRetiros,
                VolumenTotalOperado = volumenOperado,
                OrdenesActivas = await _context.OrdenesCompra.CountAsync(o => EstadosActivos.Contains(o.Estado)),
                OfertasActivas = await _context.OfertasVenta.CountAsync(o => EstadosActivos.Contains(o.Estado)),
                TransaccionesEjecutadas = transaccionesEjecutadas,
                VolumenPorMoneda = volumenPorMoneda,
                VolumenPorDia = volumenPorDia,
                OperacionesPorDia = volumenPorDia,
                DistribucionPorTipo = distribucionPorTipo,
                Monedas = monedas,
                MejoresRutas = mejoresRutas
            };
        }

        public async Task<ActividadRecientePaginadaDto> ObtenerActividadRecienteAsync(FiltroActividadRecienteDto filtro)
        {
            var desde = filtro.FechaDesde?.Date ?? DateTime.Today.AddDays(-30);
            var hastaExclusivo = filtro.FechaHasta?.Date.AddDays(1) ?? DateTime.Today.AddDays(1);

            var todos = await ConstruirActividadRecienteAsync(desde, hastaExclusivo);

            var total = todos.Count;
            var porPagina = filtro.RegistrosPorPagina <= 0 ? 20 : filtro.RegistrosPorPagina;
            var totalPaginas = total == 0 ? 1 : (int)Math.Ceiling(total / (decimal)porPagina);
            var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var registros = todos.Skip((pagina - 1) * porPagina).Take(porPagina).ToList();

            return new ActividadRecientePaginadaDto
            {
                Registros = registros,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = total,
                Mensaje = total == 0 ? "No existen operaciones para el período seleccionado" : string.Empty
            };
        }

        public async Task<List<ActividadRecienteAdminDto>> ObtenerActividadRecienteParaExportarAsync(
            DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var desde = fechaDesde?.Date ?? DateTime.Today.AddDays(-30);
            var hastaExclusivo = fechaHasta?.Date.AddDays(1) ?? DateTime.Today.AddDays(1);
            return await ConstruirActividadRecienteAsync(desde, hastaExclusivo);
        }

        private async Task<List<ActividadRecienteAdminDto>> ConstruirActividadRecienteAsync(
            DateTime desde, DateTime hastaExclusivo)
        {
            var ordenes = await _context.OrdenesCompra
                .Include(o => o.Usuario)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
                .Where(o => o.FechaCreacion >= desde && o.FechaCreacion < hastaExclusivo)
                .Select(o => new ActividadRecienteAdminDto
                {
                    FechaHora = o.FechaCreacion,
                    Usuario = o.Usuario.NombreUsuario,
                    TipoOperacion = "Orden de compra",
                    Par = o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
                    MontoTotal = o.TotalComprometido,
                    Estado = o.Estado
                })
                .ToListAsync();

            var ofertas = await _context.OfertasVenta
                .Include(o => o.Usuario)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
                .Where(o => o.FechaCreacion >= desde && o.FechaCreacion < hastaExclusivo)
                .Select(o => new ActividadRecienteAdminDto
                {
                    FechaHora = o.FechaCreacion,
                    Usuario = o.Usuario.NombreUsuario,
                    TipoOperacion = "Oferta de venta",
                    Par = o.ParMoneda.MonedaOrigen.CodigoIso + "/" + o.ParMoneda.MonedaDestino.CodigoIso,
                    MontoTotal = o.TotalEsperado,
                    Estado = o.Estado
                })
                .ToListAsync();

            var inmediatas = await _context.OperacionesInmediatas
                .Include(i => i.Usuario)
                .Include(i => i.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(i => i.ParMoneda).ThenInclude(p => p.MonedaDestino)
                .Where(i => i.OperacionPadreId == null
                    && i.FechaOperacion >= desde && i.FechaOperacion < hastaExclusivo)
                .Select(i => new ActividadRecienteAdminDto
                {
                    FechaHora = i.FechaOperacion,
                    Usuario = i.Usuario.NombreUsuario,
                    TipoOperacion = i.TipoOperacion,
                    Par = i.ParMoneda.MonedaOrigen.CodigoIso + "/" + i.ParMoneda.MonedaDestino.CodigoIso,
                    MontoTotal = i.TotalPagado ?? i.TotalRecibido ?? 0m,
                    Estado = i.Estado
                })
                .ToListAsync();

            var depositos = await _context.Depositos
                .Include(d => d.Usuario)
                .Include(d => d.Moneda)
                .Where(d => d.FechaDeposito >= desde && d.FechaDeposito < hastaExclusivo)
                .Select(d => new ActividadRecienteAdminDto
                {
                    FechaHora = d.FechaDeposito,
                    Usuario = d.Usuario.NombreUsuario,
                    TipoOperacion = "Deposito",
                    Par = d.Moneda.CodigoIso,
                    MontoTotal = d.MontoDepositado,
                    Estado = d.Estado
                })
                .ToListAsync();

            var retiros = await _context.Retiros
                .Include(r => r.Usuario)
                .Include(r => r.Moneda)
                .Where(r => r.FechaRetiro >= desde && r.FechaRetiro < hastaExclusivo)
                .Select(r => new ActividadRecienteAdminDto
                {
                    FechaHora = r.FechaRetiro,
                    Usuario = r.Usuario.NombreUsuario,
                    TipoOperacion = "Retiro",
                    Par = r.Moneda.CodigoIso,
                    MontoTotal = r.MontoRetirado,
                    Estado = r.Estado
                })
                .ToListAsync();

            return ordenes.Concat(ofertas).Concat(inmediatas).Concat(depositos).Concat(retiros)
                .OrderByDescending(x => x.FechaHora)
                .ToList();
        }

        private async Task<ParesMoneda> ObtenerParAsync(int parMonedaId)
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId && p.Activo);

            if (par == null)
                throw new ArgumentException("El par de monedas no existe.");

            return par;
        }

        private async Task<ParesMoneda> ObtenerParInversoAsync(ParesMoneda par)
        {
            var inverso = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.MonedaOrigenId == par.MonedaDestinoId && p.MonedaDestinoId == par.MonedaOrigenId && p.Activo);

            if (inverso == null)
                throw new InvalidOperationException("No existe el par inverso necesario para registrar el espejo.");

            return inverso;
        }

        private async Task<List<OfertasVenta>> ObtenerOfertasCompatiblesAsync(int parMonedaId, decimal precioMaximo, int usuarioId)
        {
            return await _context.OfertasVenta
                .Include(o => o.OrdenCompraEspejo)
                .Where(o => o.ParMonedaId == parMonedaId && o.UsuarioId != usuarioId && o.CantidadPendiente > 0 && EstadosActivos.Contains(o.Estado) && o.PrecioUnitario <= precioMaximo)
                .OrderBy(o => o.PrecioUnitario)
                .ThenBy(o => o.FechaCreacion)
                .ToListAsync();
        }

        private async Task<List<OrdenesCompra>> ObtenerOrdenesCompatiblesAsync(int parMonedaId, decimal precioMinimo, int usuarioId)
        {
            return await _context.OrdenesCompra
                .Include(o => o.OfertasVentaEspejo)
                .Where(o => o.ParMonedaId == parMonedaId && o.UsuarioId != usuarioId && o.CantidadPendiente > 0 && EstadosActivos.Contains(o.Estado) && o.PrecioUnitario >= precioMinimo)
                .OrderByDescending(o => o.PrecioUnitario)
                .ThenBy(o => o.FechaCreacion)
                .ToListAsync();
        }

        private async Task<decimal> ObtenerSaldoDisponibleAsync(int usuarioId, int monedaId)
        {
            return await _context.SaldosBilletera
                .Include(s => s.Billetera)
                .Where(s => s.Billetera.UsuarioId == usuarioId && s.MonedaId == monedaId)
                .Select(s => s.SaldoDisponible)
                .FirstOrDefaultAsync();
        }

        private async Task<SaldosBilletera> ObtenerSaldoEntityAsync(int usuarioId, int monedaId)
        {
            var billetera = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
            if (billetera == null)
            {
                billetera = new Billeteras { UsuarioId = usuarioId, FechaCreacion = DateTime.Now };
                _context.Billeteras.Add(billetera);
                await _context.SaveChangesAsync();
            }

            var saldo = await _context.SaldosBilletera.FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId && s.MonedaId == monedaId);
            if (saldo == null)
            {
                saldo = new SaldosBilletera { BilleteraId = billetera.BilleteraId, MonedaId = monedaId, SaldoDisponible = 0, FechaActualizacion = DateTime.Now };
                _context.SaldosBilletera.Add(saldo);
                await _context.SaveChangesAsync();
            }

            return saldo;
        }

        private async Task<decimal> AjustarReservaOrdenYReembolsarMejorPrecioAsync(OrdenesCompra orden, int monedaOrigenId, DateTime fecha)
        {
            var reservaNecesaria = orden.CantidadPendiente * orden.PrecioUnitario;
            var totalNuevoComprometido = orden.TotalEjecutado + reservaNecesaria;
            var reembolso = orden.TotalComprometido - totalNuevoComprometido;

            if (reembolso > 0)
            {
                var saldo = await ObtenerSaldoEntityAsync(orden.UsuarioId, monedaOrigenId);
                var anterior = saldo.SaldoDisponible;
                saldo.SaldoDisponible += reembolso;
                saldo.FechaActualizacion = fecha;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = orden.UsuarioId,
                    MonedaId = monedaOrigenId,
                    TipoMovimiento = "Reembolso",
                    Monto = reembolso,
                    SaldoAnterior = anterior,
                    SaldoPosterior = saldo.SaldoDisponible,
                    FechaMovimiento = fecha,
                    ReferenciaTipo = "OrdenCompra",
                    ReferenciaId = orden.OrdenCompraId
                });
            }

            orden.TotalComprometido = totalNuevoComprometido;
            orden.Estado = orden.CantidadPendiente <= 0 ? "Completada" : orden.CantidadObtenida > 0 ? "Parcialmente ejecutada" : "Activa";
            orden.FechaActualizacion = fecha;

            return reembolso > 0 ? reembolso : 0m;
        }

        private async Task<int?> CrearOfertaEspejoSiOrdenQuedoPendienteAsync(OrdenesCompra orden, ParesMoneda par, DateTime fecha)
        {
            if (orden.CantidadPendiente <= 0)
                return null;

            var inverso = await ObtenerParInversoAsync(par);
            var cantidadOrigenReservada = orden.CantidadPendiente * orden.PrecioUnitario;

            var espejo = new OfertasVenta
            {
                UsuarioId = orden.UsuarioId,
                ParMonedaId = inverso.ParMonedaId,
                CantidadOriginal = cantidadOrigenReservada,
                CantidadVendida = 0,
                CantidadPendiente = cantidadOrigenReservada,
                PrecioUnitario = 1 / orden.PrecioUnitario,
                TotalEsperado = orden.CantidadPendiente,
                TotalRecibido = 0,
                Estado = orden.Estado,
                FechaCreacion = fecha,
                FechaActualizacion = fecha,
                OrdenCompraEspejoId = orden.OrdenCompraId
            };

            _context.OfertasVenta.Add(espejo);
            await _context.SaveChangesAsync();
            return espejo.OfertaVentaId;
        }

        private async Task<int?> CrearOrdenEspejoSiOfertaQuedoPendienteAsync(OfertasVenta oferta, ParesMoneda par, DateTime fecha)
        {
            if (oferta.CantidadPendiente <= 0)
                return null;

            var inverso = await ObtenerParInversoAsync(par);
            var cantidadOrigenEsperada = oferta.CantidadPendiente * oferta.PrecioUnitario;

            var ordenEspejo = new OrdenesCompra
            {
                UsuarioId = oferta.UsuarioId,
                ParMonedaId = inverso.ParMonedaId,
                CantidadOriginal = cantidadOrigenEsperada,
                CantidadObtenida = 0,
                CantidadPendiente = cantidadOrigenEsperada,
                PrecioUnitario = 1 / oferta.PrecioUnitario,
                TotalComprometido = oferta.CantidadPendiente,
                TotalEjecutado = 0,
                Estado = oferta.Estado,
                FechaCreacion = fecha,
                FechaActualizacion = fecha
            };

            _context.OrdenesCompra.Add(ordenEspejo);
            await _context.SaveChangesAsync();

            oferta.OrdenCompraEspejoId = ordenEspejo.OrdenCompraId;
            return ordenEspejo.OrdenCompraId;
        }

        private async Task SincronizarOrdenEspejoExactaDesdeOfertaAsync(OfertasVenta oferta)
        {
            if (oferta.OrdenCompraEspejoId == null)
                return;

            var orden = await _context.OrdenesCompra.FirstOrDefaultAsync(o => o.OrdenCompraId == oferta.OrdenCompraEspejoId.Value);
            if (orden == null)
                return;

            orden.CantidadOriginal = oferta.TotalRecibido + (oferta.CantidadPendiente * oferta.PrecioUnitario);
            orden.CantidadObtenida = oferta.TotalRecibido;
            orden.CantidadPendiente = oferta.CantidadPendiente * oferta.PrecioUnitario;
            orden.TotalComprometido = oferta.CantidadOriginal;
            orden.TotalEjecutado = oferta.CantidadVendida;
            orden.Estado = oferta.Estado;
            orden.FechaActualizacion = DateTime.Now;
        }

        private async Task SincronizarOfertaEspejoExactaDesdeOrdenAsync(OrdenesCompra orden)
        {
            var oferta = await _context.OfertasVenta.FirstOrDefaultAsync(o => o.OrdenCompraEspejoId == orden.OrdenCompraId);
            if (oferta == null)
                return;

            oferta.CantidadOriginal = orden.TotalComprometido;
            oferta.CantidadVendida = orden.TotalEjecutado;
            oferta.CantidadPendiente = Math.Max(0, orden.TotalComprometido - orden.TotalEjecutado);
            oferta.TotalEsperado = orden.CantidadOriginal;
            oferta.TotalRecibido = orden.CantidadObtenida;
            oferta.Estado = orden.Estado;
            oferta.FechaActualizacion = DateTime.Now;
        }

        private async Task CrearNotificacionAsync(int usuarioId, string tipoEvento, string asunto, string cuerpo, string referenciaTipo, int referenciaId)
        {
            var usuario = await _context.Usuarios.FirstAsync(u => u.UsuarioId == usuarioId);
            var tipo = await _context.TiposNotificacion.FirstOrDefaultAsync(t => t.Nombre == tipoEvento);

            _context.NotificacionesCorreo.Add(new NotificacionesCorreo
            {
                UsuarioId = usuarioId,
                CorreoDestino = usuario.CorreoElectronico,
                TipoEvento = tipoEvento,
                TipoNotificacionId = tipo?.TipoNotificacionId,
                Asunto = asunto,
                Cuerpo = cuerpo,
                EstadoEnvio = "Pendiente",
                FechaCreacion = DateTime.Now,
                ReferenciaTipo = referenciaTipo,
                ReferenciaId = referenciaId
            });
        }
    }
}
