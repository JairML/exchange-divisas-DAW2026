using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.VentaInmediata;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class VentaInmediataRepository : IVentaInmediataRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public VentaInmediataRepository(ExchangeDivisasDbContext context)
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

    public async Task<List<OrdenesCompra>> ObtenerOrdenesCompraActivasAsync(int parMonedaId)
    {
        return await _context.OrdenesCompra
            .Where(o => o.ParMonedaId == parMonedaId
                     && (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada")
                     && o.CantidadPendiente > 0)
            .OrderByDescending(o => o.PrecioUnitario)
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
            TipoOperacion = "Venta",
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
                                   && b.TipoOperacion == "Venta");
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
                                   && b.TipoOperacion == "Venta");
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
        ResultadoBusquedaRutaVentaDto resultado)
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
        busqueda.TotalNormal = resultado.TotalVentaNormal;
        busqueda.TotalRuta = resultado.TotalRutaEncontrada;
        busqueda.GananciaEstimada = resultado.GananciaEstimada;

        var rutaConversion = new RutasConversion
        {
            BusquedaRutaId = busquedaRutaId,
            MonedaInicialId = busqueda.ParMoneda.MonedaOrigenId,
            MonedaFinalId = busqueda.ParMoneda.MonedaDestinoId,
            CantidadSaltos = resultado.CantidadSaltos,
            TotalEstimado = resultado.TotalRutaEncontrada,
            GananciaEstimada = resultado.GananciaEstimada,
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
                CantidadConvertida = salto.CantidadVendida,
                ResultadoObtenido = salto.ResultadoObtenido,
                PrecioMinimo = salto.PrecioMinimo,
                PrecioMaximo = salto.PrecioMaximo,
                PrecioPromedio = salto.PrecioPromedio
            };
            _context.RutaConversionSaltos.Add(saltoEntity);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<VentaInmediataResponseDto> EjecutarVentaInmediataNormalAsync(
        int usuarioId,
        int parMonedaId,
        decimal cantidadAVender,
        bool venderCantidadDisponible)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId)
                ?? throw new InvalidOperationException("Par de monedas no encontrado.");

            var ordenes = await _context.OrdenesCompra
                .Where(o => o.ParMonedaId == parMonedaId
                         && (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada")
                         && o.CantidadPendiente > 0)
                .OrderByDescending(o => o.PrecioUnitario)
                .ToListAsync();

            var billeteraVendedor = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
                ?? throw new InvalidOperationException("Billetera del vendedor no encontrada.");

            var saldoOrigenVendedor = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billeteraVendedor.BilleteraId
                                       && s.MonedaId == par.MonedaOrigenId)
                ?? throw new InvalidOperationException("Saldo insuficiente en moneda de origen.");

            var operacion = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = parMonedaId,
                TipoOperacion = "Venta inmediata",
                MetodoEjecucion = "Normal",
                CantidadSolicitada = cantidadAVender,
                CantidadEjecutada = 0m,
                Estado = "Completada",
                FechaOperacion = DateTime.UtcNow
            };
            _context.OperacionesInmediatas.Add(operacion);
            await _context.SaveChangesAsync();

            decimal cantidadRestante = cantidadAVender;
            decimal totalRecibido = 0m;
            var ejecuciones = new List<DetalleEjecucionVentaDto>();
            var preciosUsados = new List<decimal>();

            foreach (var orden in ordenes)
            {
                if (cantidadRestante <= 0m) break;

                var cantidadTomada = Math.Min(cantidadRestante, orden.CantidadPendiente);
                var subTotal = cantidadTomada * orden.PrecioUnitario;

                // Actualizar orden de compra del comprador
                orden.CantidadObtenida += cantidadTomada;
                orden.CantidadPendiente -= cantidadTomada;
                orden.TotalEjecutado += subTotal;
                orden.FechaActualizacion = DateTime.UtcNow;
                orden.Estado = orden.CantidadPendiente == 0 ? "Completada" : "Parcialmente ejecutada";

                // Crear oferta de venta temporal
                var ofertaTemporal = new OfertasVenta
                {
                    UsuarioId = usuarioId,
                    ParMonedaId = parMonedaId,
                    CantidadOriginal = cantidadTomada,
                    CantidadVendida = cantidadTomada,
                    CantidadPendiente = 0m,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalEsperado = subTotal,
                    TotalRecibido = subTotal,
                    Estado = "Completada",
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.OfertasVenta.Add(ofertaTemporal);
                await _context.SaveChangesAsync();

                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = ofertaTemporal.OfertaVentaId,
                    ParMonedaId = parMonedaId,
                    CompradorId = orden.UsuarioId,
                    VendedorId = usuarioId,
                    CantidadEjecutada = cantidadTomada,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalOperacion = subTotal,
                    FechaEjecucion = DateTime.UtcNow
                };
                _context.EjecucionesOrden.Add(ejecucion);
                await _context.SaveChangesAsync();

                _context.OperacionInmediataEjecuciones.Add(new OperacionInmediataEjecuciones
                {
                    OperacionInmediataId = operacion.OperacionInmediataId,
                    EjecucionId = ejecucion.EjecucionId
                });

                // Acreditar moneda destino al comprador de la orden
                var billeteraComprador = await _context.Billeteras
                    .FirstOrDefaultAsync(b => b.UsuarioId == orden.UsuarioId);

                if (billeteraComprador != null)
                {
                    var saldoDestinoComprador = await _context.SaldosBilletera
                        .FirstOrDefaultAsync(s => s.BilleteraId == billeteraComprador.BilleteraId
                                               && s.MonedaId == par.MonedaDestinoId);

                    if (saldoDestinoComprador == null)
                    {
                        _context.SaldosBilletera.Add(new SaldosBilletera
                        {
                            BilleteraId = billeteraComprador.BilleteraId,
                            MonedaId = par.MonedaDestinoId,
                            SaldoDisponible = cantidadTomada,
                            FechaActualizacion = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        var saldoAntComprador = saldoDestinoComprador.SaldoDisponible;
                        saldoDestinoComprador.SaldoDisponible += cantidadTomada;
                        saldoDestinoComprador.FechaActualizacion = DateTime.UtcNow;

                        _context.MovimientosBilletera.Add(new MovimientosBilletera
                        {
                            UsuarioId = orden.UsuarioId,
                            MonedaId = par.MonedaDestinoId,
                            TipoMovimiento = "Compra",
                            Monto = cantidadTomada,
                            SaldoAnterior = saldoAntComprador,
                            SaldoPosterior = saldoDestinoComprador.SaldoDisponible,
                            FechaMovimiento = DateTime.UtcNow,
                            ReferenciaTipo = "EjecucionOrden",
                            ReferenciaId = ejecucion.EjecucionId
                        });
                    }
                }

                cantidadRestante -= cantidadTomada;
                totalRecibido += subTotal;
                preciosUsados.Add(orden.PrecioUnitario);

                ejecuciones.Add(new DetalleEjecucionVentaDto
                {
                    EjecucionId = ejecucion.EjecucionId,
                    OrdenCompraId = orden.OrdenCompraId,
                    CompradorId = orden.UsuarioId,
                    CantidadEjecutada = cantidadTomada,
                    PrecioUnitario = orden.PrecioUnitario,
                    TotalOperacion = subTotal,
                    FechaEjecucion = ejecucion.FechaEjecucion
                });
            }

            decimal cantidadEjecutada = cantidadAVender - cantidadRestante;

            // Descontar saldo origen del vendedor
            var saldoAntOrigen = saldoOrigenVendedor.SaldoDisponible;
            saldoOrigenVendedor.SaldoDisponible -= cantidadEjecutada;
            saldoOrigenVendedor.FechaActualizacion = DateTime.UtcNow;

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaOrigenId,
                TipoMovimiento = "Venta",
                Monto = cantidadEjecutada,
                SaldoAnterior = saldoAntOrigen,
                SaldoPosterior = saldoOrigenVendedor.SaldoDisponible,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId
            });

            // Acreditar moneda destino al vendedor
            var saldoDestinoVendedor = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billeteraVendedor.BilleteraId
                                       && s.MonedaId == par.MonedaDestinoId);

            if (saldoDestinoVendedor == null)
            {
                saldoDestinoVendedor = new SaldosBilletera
                {
                    BilleteraId = billeteraVendedor.BilleteraId,
                    MonedaId = par.MonedaDestinoId,
                    SaldoDisponible = totalRecibido,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.SaldosBilletera.Add(saldoDestinoVendedor);
                await _context.SaveChangesAsync();
            }
            else
            {
                var saldoAntDest = saldoDestinoVendedor.SaldoDisponible;
                saldoDestinoVendedor.SaldoDisponible += totalRecibido;
                saldoDestinoVendedor.FechaActualizacion = DateTime.UtcNow;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = par.MonedaDestinoId,
                    TipoMovimiento = "Recepcion venta",
                    Monto = totalRecibido,
                    SaldoAnterior = saldoAntDest,
                    SaldoPosterior = saldoDestinoVendedor.SaldoDisponible,
                    FechaMovimiento = DateTime.UtcNow,
                    ReferenciaTipo = "OperacionInmediata",
                    ReferenciaId = operacion.OperacionInmediataId
                });
            }

            operacion.CantidadEjecutada = cantidadEjecutada;
            operacion.TotalPagado = cantidadEjecutada;
            operacion.TotalRecibido = totalRecibido;
            operacion.PrecioMinimo = preciosUsados.Count > 0 ? preciosUsados.Min() : null;
            operacion.PrecioMaximo = preciosUsados.Count > 0 ? preciosUsados.Max() : null;
            operacion.PrecioPromedio = cantidadEjecutada > 0 ? totalRecibido / cantidadEjecutada : null;

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Venta inmediata",
                ReferenciaId = operacion.OperacionInmediataId,
                ParMonedaId = parMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = "Normal"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new VentaInmediataResponseDto
            {
                OperacionInmediataId = operacion.OperacionInmediataId,
                ParMonedaId = parMonedaId,
                TipoOperacion = "Venta inmediata",
                MetodoEjecucion = "Normal",
                MonedaOrigen = par.MonedaOrigen.CodigoIso,
                MonedaDestino = par.MonedaDestino.CodigoIso,
                CantidadSolicitada = cantidadAVender,
                CantidadEjecutada = cantidadEjecutada,
                PrecioMinimo = operacion.PrecioMinimo,
                PrecioMaximo = operacion.PrecioMaximo,
                PrecioPromedio = operacion.PrecioPromedio,
                TotalRecibido = totalRecibido,
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

    public async Task<VentaInmediataResponseDto> EjecutarVentaInmediataPorRutaAsync(
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

            var operacionPadre = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = busqueda.ParMonedaId,
                TipoOperacion = "Venta inmediata",
                MetodoEjecucion = "Ruta",
                CantidadSolicitada = busqueda.CantidadSolicitada,
                CantidadEjecutada = 0m,
                Estado = "Completada",
                FechaOperacion = DateTime.UtcNow
            };
            _context.OperacionesInmediatas.Add(operacionPadre);
            await _context.SaveChangesAsync();

            decimal cantidadActual = busqueda.CantidadSolicitada;
            decimal totalRecibidoTotal = 0m;

            foreach (var salto in saltos)
            {
                var parSalto = salto.ParMoneda;

                var operacionHija = new OperacionesInmediatas
                {
                    UsuarioId = usuarioId,
                    ParMonedaId = salto.ParMonedaId,
                    TipoOperacion = "Venta inmediata",
                    MetodoEjecucion = "Ruta - salto",
                    CantidadSolicitada = cantidadActual,
                    CantidadEjecutada = 0m,
                    Estado = "Completada",
                    OperacionPadreId = operacionPadre.OperacionInmediataId,
                    FechaOperacion = DateTime.UtcNow
                };
                _context.OperacionesInmediatas.Add(operacionHija);
                await _context.SaveChangesAsync();

                salto.OperacionInmediataId = operacionPadre.OperacionInmediataId;
                salto.OperacionInmediataHijaId = operacionHija.OperacionInmediataId;

                var ordenes = await _context.OrdenesCompra
                    .Where(o => o.ParMonedaId == salto.ParMonedaId
                             && (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada")
                             && o.CantidadPendiente > 0)
                    .OrderByDescending(o => o.PrecioUnitario)
                    .ToListAsync();

                var billeteraVendedor = await _context.Billeteras
                    .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
                    ?? throw new InvalidOperationException("Billetera del vendedor no encontrada.");

                var saldoOrigen = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s => s.BilleteraId == billeteraVendedor.BilleteraId
                                           && s.MonedaId == parSalto.MonedaOrigenId)
                    ?? throw new InvalidOperationException("Saldo insuficiente para el salto.");

                decimal cantidadVender = cantidadActual;
                decimal cantidadRestante = cantidadVender;
                decimal totalRecibidoSalto = 0m;
                decimal cantidadEjecutadaSalto = 0m;
                var preciosSalto = new List<decimal>();

                foreach (var orden in ordenes)
                {
                    if (cantidadRestante <= 0m) break;

                    var cantidadTomada = Math.Min(cantidadRestante, orden.CantidadPendiente);
                    var subTotal = cantidadTomada * orden.PrecioUnitario;

                    orden.CantidadObtenida += cantidadTomada;
                    orden.CantidadPendiente -= cantidadTomada;
                    orden.TotalEjecutado += subTotal;
                    orden.FechaActualizacion = DateTime.UtcNow;
                    orden.Estado = orden.CantidadPendiente == 0 ? "Completada" : "Parcialmente ejecutada";

                    var ofertaTemporal = new OfertasVenta
                    {
                        UsuarioId = usuarioId,
                        ParMonedaId = salto.ParMonedaId,
                        CantidadOriginal = cantidadTomada,
                        CantidadVendida = cantidadTomada,
                        CantidadPendiente = 0m,
                        PrecioUnitario = orden.PrecioUnitario,
                        TotalEsperado = subTotal,
                        TotalRecibido = subTotal,
                        Estado = "Completada",
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.OfertasVenta.Add(ofertaTemporal);
                    await _context.SaveChangesAsync();

                    var ejecucion = new EjecucionesOrden
                    {
                        OrdenCompraId = orden.OrdenCompraId,
                        OfertaVentaId = ofertaTemporal.OfertaVentaId,
                        ParMonedaId = salto.ParMonedaId,
                        CompradorId = orden.UsuarioId,
                        VendedorId = usuarioId,
                        CantidadEjecutada = cantidadTomada,
                        PrecioUnitario = orden.PrecioUnitario,
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
                    totalRecibidoSalto += subTotal;
                    preciosSalto.Add(orden.PrecioUnitario);
                }

                // Descontar moneda origen del vendedor
                var saldoAntOrigen = saldoOrigen.SaldoDisponible;
                saldoOrigen.SaldoDisponible -= cantidadEjecutadaSalto;
                saldoOrigen.FechaActualizacion = DateTime.UtcNow;

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = parSalto.MonedaOrigenId,
                    TipoMovimiento = "Venta ruta",
                    Monto = cantidadEjecutadaSalto,
                    SaldoAnterior = saldoAntOrigen,
                    SaldoPosterior = saldoOrigen.SaldoDisponible,
                    FechaMovimiento = DateTime.UtcNow,
                    ReferenciaTipo = "OperacionInmediata",
                    ReferenciaId = operacionHija.OperacionInmediataId
                });

                // Acreditar moneda destino al vendedor
                var saldoDestino = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s => s.BilleteraId == billeteraVendedor.BilleteraId
                                           && s.MonedaId == parSalto.MonedaDestinoId);

                if (saldoDestino == null)
                {
                    saldoDestino = new SaldosBilletera
                    {
                        BilleteraId = billeteraVendedor.BilleteraId,
                        MonedaId = parSalto.MonedaDestinoId,
                        SaldoDisponible = totalRecibidoSalto,
                        FechaActualizacion = DateTime.UtcNow
                    };
                    _context.SaldosBilletera.Add(saldoDestino);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var saldoAntDest = saldoDestino.SaldoDisponible;
                    saldoDestino.SaldoDisponible += totalRecibidoSalto;
                    saldoDestino.FechaActualizacion = DateTime.UtcNow;

                    _context.MovimientosBilletera.Add(new MovimientosBilletera
                    {
                        UsuarioId = usuarioId,
                        MonedaId = parSalto.MonedaDestinoId,
                        TipoMovimiento = "Recepcion venta ruta",
                        Monto = totalRecibidoSalto,
                        SaldoAnterior = saldoAntDest,
                        SaldoPosterior = saldoDestino.SaldoDisponible,
                        FechaMovimiento = DateTime.UtcNow,
                        ReferenciaTipo = "OperacionInmediata",
                        ReferenciaId = operacionHija.OperacionInmediataId
                    });
                }

                operacionHija.CantidadEjecutada = cantidadEjecutadaSalto;
                operacionHija.TotalPagado = cantidadEjecutadaSalto;
                operacionHija.TotalRecibido = totalRecibidoSalto;
                operacionHija.PrecioMinimo = preciosSalto.Count > 0 ? preciosSalto.Min() : null;
                operacionHija.PrecioMaximo = preciosSalto.Count > 0 ? preciosSalto.Max() : null;
                operacionHija.PrecioPromedio = cantidadEjecutadaSalto > 0 ? totalRecibidoSalto / cantidadEjecutadaSalto : null;

                if (salto.NumeroSalto == saltos.Count)
                    totalRecibidoTotal = totalRecibidoSalto;

                cantidadActual = totalRecibidoSalto;
            }

            operacionPadre.CantidadEjecutada = busqueda.CantidadSolicitada;
            operacionPadre.TotalPagado = busqueda.CantidadSolicitada;
            operacionPadre.TotalRecibido = totalRecibidoTotal;

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Venta inmediata",
                ReferenciaId = operacionPadre.OperacionInmediataId,
                ParMonedaId = busqueda.ParMonedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = "Ruta"
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new VentaInmediataResponseDto
            {
                OperacionInmediataId = operacionPadre.OperacionInmediataId,
                ParMonedaId = busqueda.ParMonedaId,
                TipoOperacion = "Venta inmediata",
                MetodoEjecucion = "Ruta",
                MonedaOrigen = busqueda.ParMoneda.MonedaOrigen.CodigoIso,
                MonedaDestino = busqueda.ParMoneda.MonedaDestino.CodigoIso,
                CantidadSolicitada = busqueda.CantidadSolicitada,
                CantidadEjecutada = busqueda.CantidadSolicitada,
                TotalRecibido = totalRecibidoTotal,
                Estado = "Completada",
                FechaOperacion = operacionPadre.FechaOperacion,
                Ejecuciones = new List<DetalleEjecucionVentaDto>()
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
