using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.CompraInmediata;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class CompraInmediataRepository : ICompraInmediataRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public CompraInmediataRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<ParesMoneda?> ObtenerParMonedaAsync(int parMonedaId)
    {
        return await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId && p.Activo);
    }

    public async Task<List<ParesMoneda>> ObtenerParesActivosAsync()
    {
        return await _context.ParesMoneda
            .Include(p => p.MonedaOrigen)
            .Include(p => p.MonedaDestino)
            .Where(p => p.Activo)
            .ToListAsync();
    }

    public async Task<List<OfertasVenta>> ObtenerOfertasVentaActivasAsync(int parMonedaId)
    {
        return await _context.OfertasVenta
            .Where(o => o.ParMonedaId == parMonedaId
                     && (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada")
                     && o.CantidadPendiente > 0)
            .OrderBy(o => o.PrecioUnitario)
            .ToListAsync();
    }

    public async Task<decimal> ObtenerSaldoDisponibleAsync(int usuarioId, int monedaId)
    {
        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

        if (billetera == null)
            return 0m;

        var saldo = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId
                                   && s.MonedaId == monedaId);

        return saldo?.SaldoDisponible ?? 0m;
    }

    public async Task<BusquedasRuta> CrearBusquedaRutaAsync(
        int usuarioId,
        int parMonedaId,
        decimal cantidadSolicitada,
        int maxSaltos,
        int tiempoEstimadoMs)
    {
        var busqueda = new BusquedasRuta
        {
            UsuarioId = usuarioId,
            ParMonedaId = parMonedaId,
            TipoOperacion = "Compra",
            CantidadSolicitada = cantidadSolicitada,
            MaxSaltos = maxSaltos,
            TiempoEstimadoMs = tiempoEstimadoMs,
            Estado = "Pendiente",
            FechaInicio = DateTime.UtcNow
        };
        _context.BusquedasRuta.Add(busqueda);
        await _context.SaveChangesAsync();
        return busqueda;
    }

    public async Task<BusquedasRuta?> ObtenerBusquedaRutaAsync(int busquedaRutaId, int usuarioId)
    {
        return await _context.BusquedasRuta
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId
                                   && b.UsuarioId == usuarioId
                                   && b.TipoOperacion == "Compra");
    }

    public async Task<BusquedasRuta?> ObtenerBusquedaRutaConSaltosAsync(int busquedaRutaId, int usuarioId)
    {
        return await _context.BusquedasRuta
            .Include(b => b.RutasConversion)
                .ThenInclude(r => r.RutaConversionSaltos)
                    .ThenInclude(s => s.ParMoneda)
                        .ThenInclude(p => p.MonedaOrigen)
            .Include(b => b.RutasConversion)
                .ThenInclude(r => r.RutaConversionSaltos)
                    .ThenInclude(s => s.ParMoneda)
                        .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId
                                   && b.UsuarioId == usuarioId
                                   && b.TipoOperacion == "Compra");
    }

    public async Task FinalizarBusquedaRutaSinResultadoAsync(int busquedaRutaId)
    {
        var busqueda = await _context.BusquedasRuta
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId);

        if (busqueda == null)
            return;

        busqueda.Estado = "Sin resultado";
        busqueda.FechaFin = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task CancelarBusquedaRutaAsync(int busquedaRutaId)
    {
        var busqueda = await _context.BusquedasRuta
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId);

        if (busqueda == null)
            return;

        busqueda.Estado = "Cancelada";
        busqueda.FechaFin = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task GuardarResultadoRutaAsync(
        int busquedaRutaId,
        ResultadoBusquedaRutaCompraDto resultado)
    {
        var busqueda = await _context.BusquedasRuta
            .Include(b => b.ParMoneda)
                .ThenInclude(p => p.MonedaOrigen)
            .Include(b => b.ParMoneda)
                .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId);

        if (busqueda == null)
            return;

        busqueda.Estado = "Completada";
        busqueda.FechaFin = DateTime.UtcNow;
        busqueda.TotalNormal = resultado.TotalCompraNormal;
        busqueda.TotalRuta = resultado.TotalRutaEncontrada;
        busqueda.AhorroEstimado = resultado.AhorroEstimado;

        var rutaConversion = new RutasConversion
        {
            BusquedaRutaId = busquedaRutaId,
            MonedaInicialId = busqueda.ParMoneda.MonedaOrigenId,
            MonedaFinalId = busqueda.ParMoneda.MonedaDestinoId,
            CantidadSaltos = resultado.CantidadSaltos,
            TotalEstimado = resultado.TotalRutaEncontrada,
            AhorroEstimado = resultado.AhorroEstimado,
            FechaCreacion = DateTime.UtcNow
        };
        _context.RutasConversion.Add(rutaConversion);
        await _context.SaveChangesAsync();

        foreach (var salto in resultado.Saltos)
        {
            var par = await _context.ParesMoneda
                .FirstOrDefaultAsync(p => p.ParMonedaId == salto.ParMonedaId);

            if (par == null) continue;

            var saltoEntity = new RutaConversionSaltos
            {
                RutaConversionId = rutaConversion.RutaConversionId,
                NumeroSalto = salto.NumeroSalto,
                ParMonedaId = salto.ParMonedaId,
                MonedaOrigenId = par.MonedaOrigenId,
                MonedaDestinoId = par.MonedaDestinoId,
                CantidadConvertida = salto.CantidadConvertida,
                ResultadoObtenido = salto.ResultadoObtenido,
                PrecioMinimo = salto.PrecioMinimo,
                PrecioMaximo = salto.PrecioMaximo,
                PrecioPromedio = salto.PrecioPromedio
            };
            _context.RutaConversionSaltos.Add(saltoEntity);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<CompraInmediataResponseDto> EjecutarCompraInmediataNormalAsync(
        int usuarioId,
        int parMonedaId,
        decimal cantidadAObtener)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId)
                ?? throw new InvalidOperationException("Par de monedas no encontrado.");

            var ofertas = await _context.OfertasVenta
                .Where(o => o.ParMonedaId == parMonedaId
                         && (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada")
                         && o.CantidadPendiente > 0)
                .OrderBy(o => o.PrecioUnitario)
                .ToListAsync();

            var billetaraComprador = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
                ?? throw new InvalidOperationException("Billetera del comprador no encontrada.");

            var saldoOrigenComprador = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billetaraComprador.BilleteraId
                                       && s.MonedaId == par.MonedaOrigenId)
                ?? throw new InvalidOperationException("Saldo insuficiente en moneda de origen.");

            var operacion = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = parMonedaId,
                TipoOperacion = "Compra inmediata",
                MetodoEjecucion = "Normal",
                CantidadSolicitada = cantidadAObtener,
                CantidadEjecutada = 0m,
                Estado = "Completada",
                FechaOperacion = DateTime.UtcNow
            };
            _context.OperacionesInmediatas.Add(operacion);
            await _context.SaveChangesAsync();

            decimal cantidadRestante = cantidadAObtener;
            decimal totalPagado = 0m;
            var ejecuciones = new List<DetalleEjecucionCompraDto>();
            var preciosUsados = new List<decimal>();

            foreach (var oferta in ofertas)
            {
                if (cantidadRestante <= 0m) break;

                var cantidadTomada = Math.Min(cantidadRestante, oferta.CantidadPendiente);
                var subTotal = cantidadTomada * oferta.PrecioUnitario;

                // Actualizar oferta
                oferta.CantidadVendida += cantidadTomada;
                oferta.CantidadPendiente -= cantidadTomada;
                oferta.TotalRecibido += subTotal;
                oferta.FechaActualizacion = DateTime.UtcNow;
                oferta.Estado = oferta.CantidadPendiente == 0 ? "Completada" : "Parcialmente ejecutada";

                // Crear ejecucion de orden
                // La oferta de venta esta ligada a una "OrdenCompra espejo" interna si aplica,
                // pero para compra inmediata creamos una OrdenCompra temporal.
                var ordenTemporal = new OrdenesCompra
                {
                    UsuarioId = usuarioId,
                    ParMonedaId = parMonedaId,
                    CantidadOriginal = cantidadTomada,
                    CantidadObtenida = cantidadTomada,
                    CantidadPendiente = 0m,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalComprometido = subTotal,
                    TotalEjecutado = subTotal,
                    Estado = "Completada",
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.OrdenesCompra.Add(ordenTemporal);
                await _context.SaveChangesAsync();

                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = ordenTemporal.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    ParMonedaId = parMonedaId,
                    CompradorId = usuarioId,
                    VendedorId = oferta.UsuarioId,
                    CantidadEjecutada = cantidadTomada,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalOperacion = subTotal,
                    FechaEjecucion = DateTime.UtcNow
                };
                _context.EjecucionesOrden.Add(ejecucion);
                await _context.SaveChangesAsync();

                var link = new OperacionInmediataEjecuciones
                {
                    OperacionInmediataId = operacion.OperacionInmediataId,
                    EjecucionId = ejecucion.EjecucionId
                };
                _context.OperacionInmediataEjecuciones.Add(link);

                // Actualizar saldos del vendedor
                var billeteraVendedor = await _context.Billeteras
                    .FirstOrDefaultAsync(b => b.UsuarioId == oferta.UsuarioId);

                if (billeteraVendedor != null)
                {
                    // Vendedor pierde moneda origen (ya habia sido bloqueada) y gana moneda destino
                    var saldoDestinoVendedor = await _context.SaldosBilletera
                        .FirstOrDefaultAsync(s => s.BilleteraId == billeteraVendedor.BilleteraId
                                               && s.MonedaId == par.MonedaDestinoId);

                    if (saldoDestinoVendedor == null)
                    {
                        _context.SaldosBilletera.Add(new SaldosBilletera
                        {
                            BilleteraId = billeteraVendedor.BilleteraId,
                            MonedaId = par.MonedaDestinoId,
                            SaldoDisponible = subTotal,
                            FechaActualizacion = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        var saldoAntVendedor = saldoDestinoVendedor.SaldoDisponible;
                        saldoDestinoVendedor.SaldoDisponible += subTotal;
                        saldoDestinoVendedor.FechaActualizacion = DateTime.UtcNow;

                        _context.MovimientosBilletera.Add(new MovimientosBilletera
                        {
                            UsuarioId = oferta.UsuarioId,
                            MonedaId = par.MonedaDestinoId,
                            TipoMovimiento = "Venta",
                            Monto = subTotal,
                            SaldoAnterior = saldoAntVendedor,
                            SaldoPosterior = saldoDestinoVendedor.SaldoDisponible,
                            FechaMovimiento = DateTime.UtcNow,
                            ReferenciaTipo = "EjecucionOrden",
                            ReferenciaId = ejecucion.EjecucionId
                        });
                    }
                }

                cantidadRestante -= cantidadTomada;
                totalPagado += subTotal;
                preciosUsados.Add(oferta.PrecioUnitario);

                ejecuciones.Add(new DetalleEjecucionCompraDto
                {
                    EjecucionId = ejecucion.EjecucionId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    VendedorId = oferta.UsuarioId,
                    CantidadEjecutada = cantidadTomada,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalOperacion = subTotal,
                    FechaEjecucion = ejecucion.FechaEjecucion
                });
            }

            decimal cantidadEjecutada = cantidadAObtener - cantidadRestante;

            // Descontar saldo del comprador (moneda origen)
            var saldoAntComprador = saldoOrigenComprador.SaldoDisponible;
            saldoOrigenComprador.SaldoDisponible -= totalPagado;
            saldoOrigenComprador.FechaActualizacion = DateTime.UtcNow;

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaOrigenId,
                TipoMovimiento = "Compra",
                Monto = totalPagado,
                SaldoAnterior = saldoAntComprador,
                SaldoPosterior = saldoOrigenComprador.SaldoDisponible,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId
            });

            // Acreditar moneda destino al comprador
            var saldoDestinoComprador = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billetaraComprador.BilleteraId
                                       && s.MonedaId == par.MonedaDestinoId);

            if (saldoDestinoComprador == null)
            {
                saldoDestinoComprador = new SaldosBilletera
                {
                    BilleteraId = billetaraComprador.BilleteraId,
                    MonedaId = par.MonedaDestinoId,
                    SaldoDisponible = cantidadEjecutada,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.SaldosBilletera.Add(saldoDestinoComprador);
                await _context.SaveChangesAsync();
            }
            else
            {
                var saldoAntDest = saldoDestinoComprador.SaldoDisponible;
                saldoDestinoComprador.SaldoDisponible += cantidadEjecutada;
                saldoDestinoComprador.FechaActualizacion = DateTime.UtcNow;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = par.MonedaDestinoId,
                    TipoMovimiento = "Recepcion compra",
                    Monto = cantidadEjecutada,
                    SaldoAnterior = saldoAntDest,
                    SaldoPosterior = saldoDestinoComprador.SaldoDisponible,
                    FechaMovimiento = DateTime.UtcNow,
                    ReferenciaTipo = "OperacionInmediata",
                    ReferenciaId = operacion.OperacionInmediataId
                });
            }

            // Actualizar la operacion inmediata
            operacion.CantidadEjecutada = cantidadEjecutada;
            operacion.TotalPagado = totalPagado;
            operacion.TotalRecibido = cantidadEjecutada;
            operacion.PrecioMinimo = preciosUsados.Count > 0 ? preciosUsados.Min() : null;
            operacion.PrecioMaximo = preciosUsados.Count > 0 ? preciosUsados.Max() : null;
            operacion.PrecioPromedio = cantidadEjecutada > 0 ? totalPagado / cantidadEjecutada : null;

            // Historial
            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Compra inmediata",
                ReferenciaId = operacion.OperacionInmediataId,
                ParMonedaId = parMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = "Normal"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new CompraInmediataResponseDto
            {
                OperacionInmediataId = operacion.OperacionInmediataId,
                ParMonedaId = parMonedaId,
                TipoOperacion = "Compra inmediata",
                MetodoEjecucion = "Normal",
                MonedaOrigen = par.MonedaOrigen.CodigoIso,
                MonedaDestino = par.MonedaDestino.CodigoIso,
                CantidadSolicitada = cantidadAObtener,
                CantidadEjecutada = cantidadEjecutada,
                PrecioMinimo = operacion.PrecioMinimo,
                PrecioMaximo = operacion.PrecioMaximo,
                PrecioPromedio = operacion.PrecioPromedio,
                TotalPagado = totalPagado,
                Estado = "Completada",
                FechaOperacion = operacion.FechaOperacion,
                Ejecuciones = ejecuciones
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<CompraInmediataResponseDto> EjecutarCompraInmediataPorRutaAsync(
        int usuarioId,
        int busquedaRutaId)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var busqueda = await _context.BusquedasRuta
                .Include(b => b.RutasConversion)
                    .ThenInclude(r => r.RutaConversionSaltos.OrderBy(s => s.NumeroSalto))
                        .ThenInclude(s => s.ParMoneda)
                            .ThenInclude(p => p.MonedaOrigen)
                .Include(b => b.RutasConversion)
                    .ThenInclude(r => r.RutaConversionSaltos)
                        .ThenInclude(s => s.ParMoneda)
                            .ThenInclude(p => p.MonedaDestino)
                .Include(b => b.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(b => b.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId && b.UsuarioId == usuarioId)
                ?? throw new InvalidOperationException("Busqueda de ruta no encontrada.");

            var ruta = busqueda.RutasConversion.FirstOrDefault()
                ?? throw new InvalidOperationException("No hay ruta guardada para esta busqueda.");

            var saltos = ruta.RutaConversionSaltos.OrderBy(s => s.NumeroSalto).ToList();

            // Operacion padre
            var operacionPadre = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = busqueda.ParMonedaId,
                TipoOperacion = "Compra inmediata",
                MetodoEjecucion = "Ruta",
                CantidadSolicitada = busqueda.CantidadSolicitada,
                CantidadEjecutada = 0m,
                Estado = "Completada",
                FechaOperacion = DateTime.UtcNow
            };
            _context.OperacionesInmediatas.Add(operacionPadre);
            await _context.SaveChangesAsync();

            decimal cantidadActual = busqueda.CantidadSolicitada;
            decimal totalPagadoTotal = 0m;

            foreach (var salto in saltos)
            {
                var parSalto = salto.ParMoneda;

                var operacionHija = new OperacionesInmediatas
                {
                    UsuarioId = usuarioId,
                    ParMonedaId = salto.ParMonedaId,
                    TipoOperacion = "Compra inmediata",
                    MetodoEjecucion = "Ruta - salto",
                    CantidadSolicitada = salto.ResultadoObtenido,
                    CantidadEjecutada = 0m,
                    Estado = "Completada",
                    OperacionPadreId = operacionPadre.OperacionInmediataId,
                    FechaOperacion = DateTime.UtcNow
                };
                _context.OperacionesInmediatas.Add(operacionHija);
                await _context.SaveChangesAsync();

                // Actualizar referencia en salto
                salto.OperacionInmediataId = operacionPadre.OperacionInmediataId;
                salto.OperacionInmediataHijaId = operacionHija.OperacionInmediataId;

                var ofertas = await _context.OfertasVenta
                    .Where(o => o.ParMonedaId == salto.ParMonedaId
                             && (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada")
                             && o.CantidadPendiente > 0)
                    .OrderBy(o => o.PrecioUnitario)
                    .ToListAsync();

                var billetaraComprador = await _context.Billeteras
                    .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
                    ?? throw new InvalidOperationException("Billetera del comprador no encontrada.");

                var saldoOrigen = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s => s.BilleteraId == billetaraComprador.BilleteraId
                                           && s.MonedaId == parSalto.MonedaOrigenId)
                    ?? throw new InvalidOperationException("Saldo insuficiente para el salto.");

                decimal cantidadNecesaria = salto.ResultadoObtenido;
                decimal cantidadRestante = cantidadNecesaria;
                decimal totalPagadoSalto = 0m;
                decimal cantidadEjecutadaSalto = 0m;
                var preciosSalto = new List<decimal>();

                foreach (var oferta in ofertas)
                {
                    if (cantidadRestante <= 0m) break;

                    var cantidadTomada = Math.Min(cantidadRestante, oferta.CantidadPendiente);
                    var subTotal = cantidadTomada * oferta.PrecioUnitario;

                    oferta.CantidadVendida += cantidadTomada;
                    oferta.CantidadPendiente -= cantidadTomada;
                    oferta.TotalRecibido += subTotal;
                    oferta.FechaActualizacion = DateTime.UtcNow;
                    oferta.Estado = oferta.CantidadPendiente == 0 ? "Completada" : "Parcialmente ejecutada";

                    var ordenTemporal = new OrdenesCompra
                    {
                        UsuarioId = usuarioId,
                        ParMonedaId = salto.ParMonedaId,
                        CantidadOriginal = cantidadTomada,
                        CantidadObtenida = cantidadTomada,
                        CantidadPendiente = 0m,
                        PrecioUnitario = oferta.PrecioUnitario,
                        TotalComprometido = subTotal,
                        TotalEjecutado = subTotal,
                        Estado = "Completada",
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.OrdenesCompra.Add(ordenTemporal);
                    await _context.SaveChangesAsync();

                    var ejecucion = new EjecucionesOrden
                    {
                        OrdenCompraId = ordenTemporal.OrdenCompraId,
                        OfertaVentaId = oferta.OfertaVentaId,
                        ParMonedaId = salto.ParMonedaId,
                        CompradorId = usuarioId,
                        VendedorId = oferta.UsuarioId,
                        CantidadEjecutada = cantidadTomada,
                        PrecioUnitario = oferta.PrecioUnitario,
                        TotalOperacion = subTotal,
                        FechaEjecucion = DateTime.UtcNow
                    };
                    _context.EjecucionesOrden.Add(ejecucion);
                    await _context.SaveChangesAsync();

                    _context.OperacionInmediataEjecuciones.Add(new OperacionInmediataEjecuciones
                    {
                        OperacionInmediataId = operacionHija.OperacionInmediataId,
                        EjecucionId = ejecucion.EjecucionId
                    });

                    cantidadRestante -= cantidadTomada;
                    cantidadEjecutadaSalto += cantidadTomada;
                    totalPagadoSalto += subTotal;
                    preciosSalto.Add(oferta.PrecioUnitario);
                }

                // Descontar moneda origen al comprador
                var saldoAntOrigen = saldoOrigen.SaldoDisponible;
                saldoOrigen.SaldoDisponible -= totalPagadoSalto;
                saldoOrigen.FechaActualizacion = DateTime.UtcNow;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = parSalto.MonedaOrigenId,
                    TipoMovimiento = "Compra ruta",
                    Monto = totalPagadoSalto,
                    SaldoAnterior = saldoAntOrigen,
                    SaldoPosterior = saldoOrigen.SaldoDisponible,
                    FechaMovimiento = DateTime.UtcNow,
                    ReferenciaTipo = "OperacionInmediata",
                    ReferenciaId = operacionHija.OperacionInmediataId
                });

                // Acreditar moneda destino al comprador
                var saldoDestino = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s => s.BilleteraId == billetaraComprador.BilleteraId
                                           && s.MonedaId == parSalto.MonedaDestinoId);

                if (saldoDestino == null)
                {
                    saldoDestino = new SaldosBilletera
                    {
                        BilleteraId = billetaraComprador.BilleteraId,
                        MonedaId = parSalto.MonedaDestinoId,
                        SaldoDisponible = cantidadEjecutadaSalto,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.SaldosBilletera.Add(saldoDestino);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var saldoAntDest = saldoDestino.SaldoDisponible;
                    saldoDestino.SaldoDisponible += cantidadEjecutadaSalto;
                    saldoDestino.FechaActualizacion = DateTime.UtcNow;

                    _context.MovimientosBilletera.Add(new MovimientosBilletera
                    {
                        UsuarioId = usuarioId,
                        MonedaId = parSalto.MonedaDestinoId,
                        TipoMovimiento = "Recepcion compra ruta",
                        Monto = cantidadEjecutadaSalto,
                        SaldoAnterior = saldoAntDest,
                        SaldoPosterior = saldoDestino.SaldoDisponible,
                        FechaMovimiento = DateTime.UtcNow,
                        ReferenciaTipo = "OperacionInmediata",
                        ReferenciaId = operacionHija.OperacionInmediataId
                    });
                }

                // Actualizar operacion hija
                operacionHija.CantidadEjecutada = cantidadEjecutadaSalto;
                operacionHija.TotalPagado = totalPagadoSalto;
                operacionHija.TotalRecibido = cantidadEjecutadaSalto;
                operacionHija.PrecioMinimo = preciosSalto.Count > 0 ? preciosSalto.Min() : null;
                operacionHija.PrecioMaximo = preciosSalto.Count > 0 ? preciosSalto.Max() : null;
                operacionHija.PrecioPromedio = cantidadEjecutadaSalto > 0 ? totalPagadoSalto / cantidadEjecutadaSalto : null;

                cantidadActual = cantidadEjecutadaSalto;
                totalPagadoTotal = salto.NumeroSalto == 1 ? totalPagadoSalto : totalPagadoTotal;
            }

            // Actualizar operacion padre
            operacionPadre.CantidadEjecutada = cantidadActual;
            operacionPadre.TotalPagado = totalPagadoTotal;
            operacionPadre.TotalRecibido = cantidadActual;

            // Historial
            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Compra inmediata",
                ReferenciaId = operacionPadre.OperacionInmediataId,
                ParMonedaId = busqueda.ParMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = "Ruta"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new CompraInmediataResponseDto
            {
                OperacionInmediataId = operacionPadre.OperacionInmediataId,
                ParMonedaId = busqueda.ParMonedaId,
                TipoOperacion = "Compra inmediata",
                MetodoEjecucion = "Ruta",
                MonedaOrigen = busqueda.ParMoneda.MonedaOrigen.CodigoIso,
                MonedaDestino = busqueda.ParMoneda.MonedaDestino.CodigoIso,
                CantidadSolicitada = busqueda.CantidadSolicitada,
                CantidadEjecutada = cantidadActual,
                TotalPagado = totalPagadoTotal,
                Estado = "Completada",
                FechaOperacion = operacionPadre.FechaOperacion,
                Ejecuciones = new List<DetalleEjecucionCompraDto>()
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
