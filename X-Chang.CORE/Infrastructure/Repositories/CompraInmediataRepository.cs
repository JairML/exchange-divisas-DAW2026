using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Data;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.CompraInmediata;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
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
                .Where(o =>
                    o.ParMonedaId == parMonedaId &&
                    o.CantidadPendiente > 0 &&
                    (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada") &&
                    !(o.OrdenCompraEspejoId != null &&
                      !_context.MovimientosBilletera.Any(m => m.ReferenciaId == o.OfertaVentaId &&
                        (m.ReferenciaTipo == "OfertaVenta" || m.ReferenciaTipo == "ofertasventa"))))
                .OrderBy(o => o.PrecioUnitario)
                .ThenBy(o => o.FechaCreacion)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerSaldoDisponibleAsync(int usuarioId, int monedaId)
        {
            return await _context.SaldosBilletera
                .Include(s => s.Billetera)
                .Where(s => s.Billetera.UsuarioId == usuarioId && s.MonedaId == monedaId)
                .Select(s => s.SaldoDisponible)
                .FirstOrDefaultAsync();
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
                TipoOperacion = "Compra inmediata",
                CantidadSolicitada = cantidadSolicitada,
                MaxSaltos = maxSaltos,
                TiempoEstimadoMs = tiempoEstimadoMs,
                Estado = "Ejecutando",
                FechaInicio = DateTime.Now
            };

            _context.BusquedasRuta.Add(busqueda);
            await _context.SaveChangesAsync();

            return busqueda;
        }

        public async Task<BusquedasRuta?> ObtenerBusquedaRutaAsync(int busquedaRutaId, int usuarioId)
        {
            return await _context.BusquedasRuta
                .FirstOrDefaultAsync(b =>
                    b.BusquedaRutaId == busquedaRutaId &&
                    b.UsuarioId == usuarioId);
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
                .FirstOrDefaultAsync(b =>
                    b.BusquedaRutaId == busquedaRutaId &&
                    b.UsuarioId == usuarioId);
        }

        public async Task FinalizarBusquedaRutaSinResultadoAsync(int busquedaRutaId)
        {
            var busqueda = await _context.BusquedasRuta.FindAsync(busquedaRutaId);

            if (busqueda == null)
                return;

            busqueda.Estado = "Sin resultado";
            busqueda.FechaFin = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task CancelarBusquedaRutaAsync(int busquedaRutaId)
        {
            var busqueda = await _context.BusquedasRuta.FindAsync(busquedaRutaId);

            if (busqueda == null)
                return;

            busqueda.Estado = "Cancelada";
            busqueda.FechaFin = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task GuardarResultadoRutaAsync(
            int busquedaRutaId,
            ResultadoBusquedaRutaCompraDto resultado)
        {
            var busqueda = await _context.BusquedasRuta
                .Include(b => b.ParMoneda)
                .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            busqueda.Estado = "Completada";
            busqueda.TotalNormal = resultado.TotalCompraNormal;
            busqueda.TotalRuta = resultado.TotalRutaEncontrada;
            busqueda.AhorroEstimado = resultado.AhorroEstimado;
            busqueda.FechaFin = DateTime.Now;

            var ruta = new RutasConversion
            {
                BusquedaRutaId = busquedaRutaId,
                MonedaInicialId = busqueda.ParMoneda.MonedaOrigenId,
                MonedaFinalId = busqueda.ParMoneda.MonedaDestinoId,
                CantidadSaltos = resultado.Saltos.Count,
                TotalEstimado = resultado.TotalRutaEncontrada,
                AhorroEstimado = resultado.AhorroEstimado,
                FechaCreacion = DateTime.Now
            };

            _context.RutasConversion.Add(ruta);
            await _context.SaveChangesAsync();

            foreach (var saltoDto in resultado.Saltos)
            {
                var par = await _context.ParesMoneda
                    .FirstAsync(p => p.ParMonedaId == saltoDto.ParMonedaId);

                var salto = new RutaConversionSaltos
                {
                    RutaConversionId = ruta.RutaConversionId,
                    NumeroSalto = saltoDto.NumeroSalto,
                    ParMonedaId = saltoDto.ParMonedaId,
                    MonedaOrigenId = par.MonedaOrigenId,
                    MonedaDestinoId = par.MonedaDestinoId,
                    CantidadConvertida = saltoDto.CantidadConvertida,
                    ResultadoObtenido = saltoDto.ResultadoObtenido,
                    PrecioMinimo = saltoDto.PrecioMinimo,
                    PrecioMaximo = saltoDto.PrecioMaximo,
                    PrecioPromedio = saltoDto.PrecioPromedio
                };

                _context.RutaConversionSaltos.Add(salto);
            }

            await _context.SaveChangesAsync();
        }


public async Task<CompraInmediataResponseDto> EjecutarCompraInmediataNormalAsync(
    int usuarioId,
    int parMonedaId,
    decimal cantidadAObtener)
{
    var connection = _context.Database.GetDbConnection();
    var debeCerrar = connection.State != ConnectionState.Open;

    if (debeCerrar)
        await connection.OpenAsync();

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "select public.ejecutar_compra_inmediata_segura(@usuarioid, @parmonedaid, @cantidad)::text";

        var pUsuario = command.CreateParameter();
        pUsuario.ParameterName = "usuarioid";
        pUsuario.Value = usuarioId;
        command.Parameters.Add(pUsuario);

        var pPar = command.CreateParameter();
        pPar.ParameterName = "parmonedaid";
        pPar.Value = parMonedaId;
        command.Parameters.Add(pPar);

        var pCantidad = command.CreateParameter();
        pCantidad.ParameterName = "cantidad";
        pCantidad.Value = cantidadAObtener;
        command.Parameters.Add(pCantidad);

        var json = (string?)await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("No se obtuvo respuesta de Supabase.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new CompraInmediataResponseDto
        {
            OperacionInmediataId = root.TryGetProperty("operacionid", out var id) ? id.GetInt32() : 0,
            ParMonedaId = parMonedaId,
            TipoOperacion = "Compra inmediata",
            MetodoEjecucion = "Normal",
            MonedaOrigen = root.TryGetProperty("monedaorigen", out var origen) ? origen.GetString() ?? string.Empty : string.Empty,
            MonedaDestino = root.TryGetProperty("monedadestino", out var destino) ? destino.GetString() ?? string.Empty : string.Empty,
            CantidadSolicitada = cantidadAObtener,
            CantidadEjecutada = root.TryGetProperty("cantidadcomprada", out var cantidadComprada) ? cantidadComprada.GetDecimal() : cantidadAObtener,
            TotalPagado = root.TryGetProperty("totalpagado", out var totalPagado) ? totalPagado.GetDecimal() : 0m,
            Estado = "Completada",
            FechaOperacion = DateTime.UtcNow,
            Ejecuciones = new List<DetalleEjecucionCompraDto>()
        };
    }
    catch (Exception ex) when (ex.Message.Contains("Saldo insuficiente", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Saldo insuficiente");
    }
    catch (Exception ex) when (ex.Message.Contains("Liquidez insuficiente", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Liquidez insuficiente");
    }
    catch (Exception ex) when (ex.Message.Contains("Valor inválido", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Valor inválido");
    }
    finally
    {
        if (debeCerrar)
            await connection.CloseAsync();
    }
}


        public async Task<CompraInmediataResponseDto> EjecutarCompraInmediataPorRutaAsync(
    int usuarioId,
    int busquedaRutaId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var busqueda = await _context.BusquedasRuta
                .Include(b => b.RutasConversion)
                    .ThenInclude(r => r.RutaConversionSaltos)
                .FirstOrDefaultAsync(b =>
                    b.BusquedaRutaId == busquedaRutaId &&
                    b.UsuarioId == usuarioId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            if (busqueda.Estado != "Completada")
                throw new InvalidOperationException("La búsqueda no tiene una ruta disponible para confirmar.");

            var ruta = busqueda.RutasConversion.FirstOrDefault();

            if (ruta == null)
                throw new InvalidOperationException("La ruta no fue registrada.");

            var parent = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = busqueda.ParMonedaId,
                TipoOperacion = "Compra inmediata",
                MetodoEjecucion = "Mejor ruta",
                CantidadSolicitada = busqueda.CantidadSolicitada,
                CantidadEjecutada = busqueda.CantidadSolicitada,
                TotalPagado = busqueda.TotalRuta ?? 0,
                Estado = "Completada",
                FechaOperacion = DateTime.Now
            };

            _context.OperacionesInmediatas.Add(parent);
            await _context.SaveChangesAsync();

            var saltos = ruta.RutaConversionSaltos
                .OrderBy(s => s.NumeroSalto)
                .ToList();

            var operacionesHijas = new List<CompraInmediataResponseDto>();

            foreach (var salto in saltos)
            {
                var child = await EjecutarCompraNormalInternaAsync(
                    usuarioId,
                    salto.ParMonedaId,
                    salto.ResultadoObtenido,
                    "Mejor ruta",
                    parent.OperacionInmediataId);

                operacionesHijas.Add(child);

                salto.OperacionInmediataHijaId = child.OperacionInmediataId;
                salto.OperacionInmediataId = parent.OperacionInmediataId;
            }

            ruta.OperacionInmediataId = parent.OperacionInmediataId;

            if (operacionesHijas.Any())
            {
                var costoInicial = operacionesHijas.First().TotalPagado;

                parent.TotalPagado = costoInicial;

                parent.PrecioMinimo = operacionesHijas
                    .Where(o => o.PrecioMinimo.HasValue)
                    .Select(o => o.PrecioMinimo!.Value)
                    .DefaultIfEmpty()
                    .Min();

                parent.PrecioMaximo = operacionesHijas
                    .Where(o => o.PrecioMaximo.HasValue)
                    .Select(o => o.PrecioMaximo!.Value)
                    .DefaultIfEmpty()
                    .Max();

                parent.PrecioPromedio = busqueda.CantidadSolicitada > 0
                    ? costoInicial / busqueda.CantidadSolicitada
                    : null;
            }

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Compra inmediata",
                ReferenciaId = parent.OperacionInmediataId,
                ParMonedaId = parent.ParMonedaId,
                Estado = "Completada",
                MetodoEjecucion = "Mejor ruta",
                FechaHora = DateTime.Now
            });

            await CrearNotificacionAsync(
                usuarioId,
                "Mejor ruta",
                "Compra mediante mejor ruta ejecutada",
                $"Tu compra mediante mejor ruta fue ejecutada por un total de {parent.TotalPagado}.",
                "OperacionInmediata",
                parent.OperacionInmediataId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await ConstruirRespuestaCompraAsync(parent.OperacionInmediataId);
        }

        private async Task<CompraInmediataResponseDto> EjecutarCompraNormalInternaAsync(
            int usuarioId,
            int parMonedaId,
            decimal cantidadAObtener,
            string metodoEjecucion,
            int? operacionPadreId)
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId);

            if (par == null)
                throw new ArgumentException("El par de monedas no existe.");

            var ofertas = await ObtenerOfertasVentaActivasAsync(parMonedaId);

            decimal cantidadPendiente = cantidadAObtener;
            decimal cantidadEjecutada = 0;
            decimal totalPagado = 0;

            var ofertasUsadas = new List<(OfertasVenta oferta, decimal cantidad, decimal total)>();

            foreach (var oferta in ofertas)
            {
                if (cantidadPendiente <= 0)
                    break;

                var cantidadTomada = Math.Min(cantidadPendiente, oferta.CantidadPendiente);
                var totalOperacion = cantidadTomada * oferta.PrecioUnitario;

                ofertasUsadas.Add((oferta, cantidadTomada, totalOperacion));

                cantidadEjecutada += cantidadTomada;
                totalPagado += totalOperacion;
                cantidadPendiente -= cantidadTomada;
            }

            if (cantidadEjecutada <= 0)
                throw new InvalidOperationException("No existe liquidez disponible.");

            if (cantidadEjecutada < cantidadAObtener)
                throw new InvalidOperationException("No existe suficiente liquidez para cubrir toda la cantidad solicitada.");

            var saldoCompradorOrigen = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaOrigenId);
            var saldoCompradorDestino = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaDestinoId);

            if (saldoCompradorOrigen.SaldoDisponible < totalPagado)
                throw new InvalidOperationException("Saldo insuficiente.");

            var saldoAnteriorCompradorOrigen = saldoCompradorOrigen.SaldoDisponible;
            var saldoAnteriorCompradorDestino = saldoCompradorDestino.SaldoDisponible;

            saldoCompradorOrigen.SaldoDisponible -= totalPagado;
            saldoCompradorDestino.SaldoDisponible += cantidadEjecutada;

            var operacion = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = parMonedaId,
                TipoOperacion = "Compra inmediata",
                MetodoEjecucion = metodoEjecucion,
                CantidadSolicitada = cantidadAObtener,
                CantidadEjecutada = cantidadEjecutada,
                PrecioMinimo = ofertasUsadas.Min(x => x.oferta.PrecioUnitario),
                PrecioMaximo = ofertasUsadas.Max(x => x.oferta.PrecioUnitario),
                PrecioPromedio = totalPagado / cantidadEjecutada,
                TotalPagado = totalPagado,
                Estado = "Completada",
                FechaOperacion = DateTime.Now,
                OperacionPadreId = operacionPadreId
            };

            _context.OperacionesInmediatas.Add(operacion);
            await _context.SaveChangesAsync();

            var ordenInterna = new OrdenesCompra
            {
                UsuarioId = usuarioId,
                ParMonedaId = parMonedaId,
                CantidadOriginal = cantidadEjecutada,
                CantidadObtenida = cantidadEjecutada,
                CantidadPendiente = 0,
                PrecioUnitario = operacion.PrecioPromedio ?? 0,
                TotalComprometido = totalPagado,
                TotalEjecutado = totalPagado,
                Estado = "Completada",
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            _context.OrdenesCompra.Add(ordenInterna);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaOrigenId,
                TipoMovimiento = "CompraInmediata",
                Monto = -totalPagado,
                SaldoAnterior = saldoAnteriorCompradorOrigen,
                SaldoPosterior = saldoCompradorOrigen.SaldoDisponible,
                FechaMovimiento = DateTime.Now,
                ReferenciaTipo = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId
            });

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaDestinoId,
                TipoMovimiento = "CompraInmediata",
                Monto = cantidadEjecutada,
                SaldoAnterior = saldoAnteriorCompradorDestino,
                SaldoPosterior = saldoCompradorDestino.SaldoDisponible,
                FechaMovimiento = DateTime.Now,
                ReferenciaTipo = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId
            });

            foreach (var item in ofertasUsadas)
            {
                var oferta = item.oferta;
                var cantidad = item.cantidad;
                var total = item.total;

                oferta.CantidadVendida += cantidad;
                oferta.CantidadPendiente -= cantidad;
                oferta.TotalRecibido += total;
                oferta.FechaActualizacion = DateTime.Now;
                oferta.Estado = oferta.CantidadPendiente == 0
                    ? "Completada"
                    : "Parcialmente ejecutada";

                await SincronizarOrdenEspejoDesdeOfertaAsync(oferta, cantidad, total);

                var saldoVendedorOrigen = await ObtenerSaldoEntityAsync(oferta.UsuarioId, par.MonedaOrigenId);
                var saldoAnteriorVendedor = saldoVendedorOrigen.SaldoDisponible;

                saldoVendedorOrigen.SaldoDisponible += total;
                saldoVendedorOrigen.FechaActualizacion = DateTime.Now;

                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = ordenInterna.OrdenCompraId,
                    OfertaVentaId = oferta.OfertaVentaId,
                    ParMonedaId = parMonedaId,
                    CompradorId = usuarioId,
                    VendedorId = oferta.UsuarioId,
                    CantidadEjecutada = cantidad,
                    PrecioUnitario = oferta.PrecioUnitario,
                    TotalOperacion = total,
                    FechaEjecucion = DateTime.Now
                };

                _context.EjecucionesOrden.Add(ejecucion);
                await _context.SaveChangesAsync();

                _context.OperacionInmediataEjecuciones.Add(new OperacionInmediataEjecuciones
                {
                    OperacionInmediataId = operacion.OperacionInmediataId,
                    EjecucionId = ejecucion.EjecucionId
                });

                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = oferta.UsuarioId,
                    MonedaId = par.MonedaOrigenId,
                    TipoMovimiento = "VentaInmediata",
                    Monto = total,
                    SaldoAnterior = saldoAnteriorVendedor,
                    SaldoPosterior = saldoVendedorOrigen.SaldoDisponible,
                    FechaMovimiento = DateTime.Now,
                    ReferenciaTipo = "EjecucionOrden",
                    ReferenciaId = ejecucion.EjecucionId
                });

                await CrearNotificacionAsync(
                    oferta.UsuarioId,
                    "Oferta parcial",
                    "Oferta ejecutada",
                    $"Tu oferta recibió una ejecución por {cantidad} unidades.",
                    "EjecucionOrden",
                    ejecucion.EjecucionId);
            }

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Compra inmediata",
                ReferenciaId = operacion.OperacionInmediataId,
                ParMonedaId = parMonedaId,
                Estado = "Completada",
                MetodoEjecucion = metodoEjecucion,
                FechaHora = DateTime.Now
            });

            await CrearNotificacionAsync(
                usuarioId,
                metodoEjecucion == "Mejor ruta" ? "Mejor ruta" : "Compra inmediata",
                "Compra inmediata ejecutada",
                $"Tu compra inmediata fue ejecutada por un total de {totalPagado}.",
                "OperacionInmediata",
                operacion.OperacionInmediataId);

            await _context.SaveChangesAsync();

            return await ConstruirRespuestaCompraAsync(operacion.OperacionInmediataId);
        }

        private async Task<SaldosBilletera> ObtenerSaldoEntityAsync(int usuarioId, int monedaId)
        {
            return await _context.SaldosBilletera
                .Include(s => s.Billetera)
                .FirstAsync(s =>
                    s.Billetera.UsuarioId == usuarioId &&
                    s.MonedaId == monedaId);
        }

        private async Task CrearNotificacionAsync(
            int usuarioId,
            string tipoEvento,
            string asunto,
            string cuerpo,
            string referenciaTipo,
            int referenciaId)
        {
            var usuario = await _context.Usuarios.FirstAsync(u => u.UsuarioId == usuarioId);

            var tipo = await _context.TiposNotificacion
                .FirstOrDefaultAsync(t => t.Nombre == tipoEvento);

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

        private async Task<CompraInmediataResponseDto> ConstruirRespuestaCompraAsync(int operacionInmediataId)
        {
            var operacion = await _context.OperacionesInmediatas
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda)
                    .ThenInclude(p => p.MonedaDestino)
                .FirstAsync(o => o.OperacionInmediataId == operacionInmediataId);

            var ejecuciones = await _context.OperacionInmediataEjecuciones
                .Include(x => x.Ejecucion)
                .Where(x => x.OperacionInmediataId == operacionInmediataId)
                .Select(x => new DetalleEjecucionCompraDto
                {
                    EjecucionId = x.EjecucionId,
                    OfertaVentaId = x.Ejecucion.OfertaVentaId,
                    VendedorId = x.Ejecucion.VendedorId,
                    CantidadEjecutada = x.Ejecucion.CantidadEjecutada,
                    PrecioUnitario = x.Ejecucion.PrecioUnitario,
                    TotalOperacion = x.Ejecucion.TotalOperacion,
                    FechaEjecucion = x.Ejecucion.FechaEjecucion
                })
                .ToListAsync();

            return new CompraInmediataResponseDto
            {
                OperacionInmediataId = operacion.OperacionInmediataId,
                ParMonedaId = operacion.ParMonedaId,
                TipoOperacion = operacion.TipoOperacion,
                MetodoEjecucion = operacion.MetodoEjecucion,
                MonedaOrigen = operacion.ParMoneda.MonedaOrigen.CodigoIso,
                MonedaDestino = operacion.ParMoneda.MonedaDestino.CodigoIso,
                CantidadSolicitada = operacion.CantidadSolicitada,
                CantidadEjecutada = operacion.CantidadEjecutada,
                PrecioMinimo = operacion.PrecioMinimo,
                PrecioMaximo = operacion.PrecioMaximo,
                PrecioPromedio = operacion.PrecioPromedio,
                TotalPagado = operacion.TotalPagado ?? 0,
                Estado = operacion.Estado,
                FechaOperacion = operacion.FechaOperacion,
                Ejecuciones = ejecuciones
            };
        }

        private async Task SincronizarOrdenEspejoDesdeOfertaAsync(
			OfertasVenta oferta,
			decimal cantidadVendida,
			decimal totalRecibido)
		{
			if (oferta.OrdenCompraEspejoId == null)
				return;

			var ordenEspejo = await _context.OrdenesCompra
				.FirstOrDefaultAsync(o => o.OrdenCompraId == oferta.OrdenCompraEspejoId.Value);

			if (ordenEspejo == null)
				return;

			ordenEspejo.CantidadOriginal = oferta.TotalRecibido + (oferta.CantidadPendiente * oferta.PrecioUnitario);
			ordenEspejo.CantidadObtenida = oferta.TotalRecibido;
			ordenEspejo.CantidadPendiente = oferta.CantidadPendiente * oferta.PrecioUnitario;
			ordenEspejo.TotalComprometido = oferta.CantidadOriginal;
			ordenEspejo.TotalEjecutado = oferta.CantidadVendida;
			ordenEspejo.FechaActualizacion = DateTime.Now;

			if (ordenEspejo.CantidadPendiente < 0)
				ordenEspejo.CantidadPendiente = 0;

			ordenEspejo.Estado = oferta.Estado;
		}
    }
}