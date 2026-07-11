using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Data;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.VentaInmediata;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
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
                .Where(o =>
                    o.ParMonedaId == parMonedaId &&
                    o.CantidadPendiente > 0 &&
                    (o.Estado == "Activa" || o.Estado == "Parcialmente ejecutada") &&
                    !(_context.OfertasVenta.Any(v => v.OrdenCompraEspejoId == o.OrdenCompraId) &&
                      !_context.MovimientosBilletera.Any(m => m.ReferenciaId == o.OrdenCompraId &&
                        (m.ReferenciaTipo == "OrdenCompra" || m.ReferenciaTipo == "ordenescompra"))))
                .OrderByDescending(o => o.PrecioUnitario)
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
                TipoOperacion = "Venta inmediata",
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
            ResultadoBusquedaRutaVentaDto resultado)
        {
            var busqueda = await _context.BusquedasRuta
                .Include(b => b.ParMoneda)
                .FirstOrDefaultAsync(b => b.BusquedaRutaId == busquedaRutaId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            busqueda.Estado = "Completada";
            busqueda.TotalNormal = resultado.TotalVentaNormal;
            busqueda.TotalRuta = resultado.TotalRutaEncontrada;
            busqueda.GananciaEstimada = resultado.GananciaEstimada;
            busqueda.FechaFin = DateTime.Now;

            var ruta = new RutasConversion
            {
                BusquedaRutaId = busquedaRutaId,
                MonedaInicialId = busqueda.ParMoneda.MonedaDestinoId,
                MonedaFinalId = busqueda.ParMoneda.MonedaOrigenId,
                CantidadSaltos = resultado.Saltos.Count,
                TotalEstimado = resultado.TotalRutaEncontrada,
                GananciaEstimada = resultado.GananciaEstimada,
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
                    CantidadConvertida = saltoDto.CantidadVendida,
                    ResultadoObtenido = saltoDto.ResultadoObtenido,
                    PrecioMinimo = saltoDto.PrecioMinimo,
                    PrecioMaximo = saltoDto.PrecioMaximo,
                    PrecioPromedio = saltoDto.PrecioPromedio
                };

                _context.RutaConversionSaltos.Add(salto);
            }

            await _context.SaveChangesAsync();
        }


        public async Task<VentaInmediataResponseDto> EjecutarVentaInmediataNormalAsync(
            int usuarioId,
            int parMonedaId,
            decimal cantidadAVender,
            bool venderCantidadDisponible)
        {
    var connection = _context.Database.GetDbConnection();
    var debeCerrar = connection.State != ConnectionState.Open;

    if (debeCerrar)
        await connection.OpenAsync();

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "select public.ejecutar_venta_inmediata_segura(@usuarioid, @parmonedaid, @cantidad)::text";

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
        pCantidad.Value = cantidadAVender;
        command.Parameters.Add(pCantidad);

        var json = (string?)await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException("No se obtuvo respuesta de Supabase.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new VentaInmediataResponseDto
        {
            OperacionInmediataId = root.TryGetProperty("operacionid", out var id) ? id.GetInt32() : 0,
            ParMonedaId = parMonedaId,
            TipoOperacion = "Venta inmediata",
            MetodoEjecucion = "Normal",
            MonedaOrigen = root.TryGetProperty("monedarecibida", out var recibida) ? recibida.GetString() ?? string.Empty : string.Empty,
            MonedaDestino = root.TryGetProperty("monedavendida", out var vendida) ? vendida.GetString() ?? string.Empty : string.Empty,
            CantidadSolicitada = cantidadAVender,
            CantidadEjecutada = root.TryGetProperty("cantidadvendida", out var cantidadVendida) ? cantidadVendida.GetDecimal() : cantidadAVender,
            TotalRecibido = root.TryGetProperty("totalrecibido", out var totalRecibido) ? totalRecibido.GetDecimal() : 0m,
            Estado = "Completada",
            FechaOperacion = DateTime.UtcNow,
            Ejecuciones = new List<DetalleEjecucionVentaDto>()
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


        public async Task<VentaInmediataResponseDto> EjecutarVentaInmediataPorRutaAsync(
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
                TipoOperacion = "Venta inmediata",
                MetodoEjecucion = "Mejor ruta",
                CantidadSolicitada = busqueda.CantidadSolicitada,
                CantidadEjecutada = busqueda.CantidadSolicitada,
                TotalRecibido = busqueda.TotalRuta ?? 0,
                Estado = "Completada",
                FechaOperacion = DateTime.Now
            };

            _context.OperacionesInmediatas.Add(parent);
            await _context.SaveChangesAsync();

            var saltos = ruta.RutaConversionSaltos
                .OrderBy(s => s.NumeroSalto)
                .ToList();

            var operacionesHijas = new List<VentaInmediataResponseDto>();

            foreach (var salto in saltos)
            {
                var child = await EjecutarVentaNormalInternaAsync(
                    usuarioId,
                    salto.ParMonedaId,
                    salto.CantidadConvertida,
                    "Mejor ruta",
                    parent.OperacionInmediataId);

                operacionesHijas.Add(child);

                salto.OperacionInmediataHijaId = child.OperacionInmediataId;
                salto.OperacionInmediataId = parent.OperacionInmediataId;
            }

            ruta.OperacionInmediataId = parent.OperacionInmediataId;

            if (operacionesHijas.Any())
            {
                var totalFinalRecibido = operacionesHijas.Last().TotalRecibido;

                parent.TotalRecibido = totalFinalRecibido;

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
                    ? totalFinalRecibido / busqueda.CantidadSolicitada
                    : null;
            }

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Venta inmediata",
                ReferenciaId = parent.OperacionInmediataId,
                ParMonedaId = parent.ParMonedaId,
                Estado = "Completada",
                MetodoEjecucion = "Mejor ruta",
                FechaHora = DateTime.Now
            });

            await CrearNotificacionAsync(
                usuarioId,
                "Mejor ruta",
                "Venta mediante mejor ruta ejecutada",
                $"Tu venta mediante mejor ruta fue ejecutada por un total de {parent.TotalRecibido}.",
                "OperacionInmediata",
                parent.OperacionInmediataId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await ConstruirRespuestaVentaAsync(parent.OperacionInmediataId);
        }

        private async Task<VentaInmediataResponseDto> EjecutarVentaNormalInternaAsync(
            int usuarioId,
            int parMonedaId,
            decimal cantidadAVender,
            string metodoEjecucion,
            int? operacionPadreId)
        {
            var par = await _context.ParesMoneda
                .Include(p => p.MonedaOrigen)
                .Include(p => p.MonedaDestino)
                .FirstOrDefaultAsync(p => p.ParMonedaId == parMonedaId);

            if (par == null)
                throw new ArgumentException("El par de monedas no existe.");

            var ordenes = await ObtenerOrdenesCompraActivasAsync(parMonedaId);

            decimal cantidadPendiente = cantidadAVender;
            decimal cantidadEjecutada = 0;
            decimal totalRecibido = 0;

            var ordenesUsadas = new List<(OrdenesCompra orden, decimal cantidad, decimal total)>();

            foreach (var orden in ordenes)
            {
                if (cantidadPendiente <= 0)
                    break;

                var cantidadTomada = Math.Min(cantidadPendiente, orden.CantidadPendiente);
                var totalOperacion = cantidadTomada * orden.PrecioUnitario;

                ordenesUsadas.Add((orden, cantidadTomada, totalOperacion));

                cantidadEjecutada += cantidadTomada;
                totalRecibido += totalOperacion;
                cantidadPendiente -= cantidadTomada;
            }

            if (cantidadEjecutada <= 0)
                throw new InvalidOperationException("No existe liquidez disponible.");

            if (cantidadEjecutada < cantidadAVender)
                throw new InvalidOperationException("No existe suficiente liquidez para cubrir toda la cantidad solicitada.");

            // En venta inmediata se vende la moneda destino del par y se recibe la moneda origen.
            // Ejemplo: en PEN/USD, el vendedor entrega USD y recibe PEN.
            var saldoVendedorDestino = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaDestinoId);
            var saldoVendedorOrigen = await ObtenerSaldoEntityAsync(usuarioId, par.MonedaOrigenId);

            if (saldoVendedorDestino.SaldoDisponible < cantidadEjecutada)
                throw new InvalidOperationException("Saldo insuficiente.");

            var saldoAnteriorVendedorDestino = saldoVendedorDestino.SaldoDisponible;
            var saldoAnteriorVendedorOrigen = saldoVendedorOrigen.SaldoDisponible;

            saldoVendedorDestino.SaldoDisponible -= cantidadEjecutada;
            saldoVendedorDestino.FechaActualizacion = DateTime.Now;

            saldoVendedorOrigen.SaldoDisponible += totalRecibido;
            saldoVendedorOrigen.FechaActualizacion = DateTime.Now;

            var operacion = new OperacionesInmediatas
            {
                UsuarioId = usuarioId,
                ParMonedaId = parMonedaId,
                TipoOperacion = "Venta inmediata",
                MetodoEjecucion = metodoEjecucion,
                CantidadSolicitada = cantidadAVender,
                CantidadEjecutada = cantidadEjecutada,
                PrecioMinimo = ordenesUsadas.Min(x => x.orden.PrecioUnitario),
                PrecioMaximo = ordenesUsadas.Max(x => x.orden.PrecioUnitario),
                PrecioPromedio = totalRecibido / cantidadEjecutada,
                TotalRecibido = totalRecibido,
                Estado = "Completada",
                FechaOperacion = DateTime.Now,
                OperacionPadreId = operacionPadreId
            };

            _context.OperacionesInmediatas.Add(operacion);
            await _context.SaveChangesAsync();

            var ofertaInterna = new OfertasVenta
            {
                UsuarioId = usuarioId,
                ParMonedaId = parMonedaId,
                CantidadOriginal = cantidadEjecutada,
                CantidadVendida = cantidadEjecutada,
                CantidadPendiente = 0,
                PrecioUnitario = operacion.PrecioPromedio ?? 0,
                TotalEsperado = totalRecibido,
                TotalRecibido = totalRecibido,
                Estado = "Completada",
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            _context.OfertasVenta.Add(ofertaInterna);
            await _context.SaveChangesAsync();

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaDestinoId,
                TipoMovimiento = "VentaInmediata",
                Monto = -cantidadEjecutada,
                SaldoAnterior = saldoAnteriorVendedorDestino,
                SaldoPosterior = saldoVendedorDestino.SaldoDisponible,
                FechaMovimiento = DateTime.Now,
                ReferenciaTipo = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId
            });

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = par.MonedaOrigenId,
                TipoMovimiento = "VentaInmediata",
                Monto = totalRecibido,
                SaldoAnterior = saldoAnteriorVendedorOrigen,
                SaldoPosterior = saldoVendedorOrigen.SaldoDisponible,
                FechaMovimiento = DateTime.Now,
                ReferenciaTipo = "OperacionInmediata",
                ReferenciaId = operacion.OperacionInmediataId
            });

            foreach (var item in ordenesUsadas)
            {
                var orden = item.orden;
                var cantidad = item.cantidad;
                var total = item.total;

                orden.CantidadObtenida += cantidad;
                orden.CantidadPendiente -= cantidad;
                orden.TotalEjecutado += total;
                orden.FechaActualizacion = DateTime.Now;
                orden.Estado = orden.CantidadPendiente == 0
                    ? "Completada"
                    : "Parcialmente ejecutada";

                await SincronizarOfertaEspejoDesdeOrdenAsync(orden, cantidad, total);

                var saldoCompradorDestino = await ObtenerSaldoEntityAsync(orden.UsuarioId, par.MonedaDestinoId);
                var saldoAnteriorCompradorDestino = saldoCompradorDestino.SaldoDisponible;

                saldoCompradorDestino.SaldoDisponible += cantidad;
                saldoCompradorDestino.FechaActualizacion = DateTime.Now;

                var ejecucion = new EjecucionesOrden
                {
                    OrdenCompraId = orden.OrdenCompraId,
                    OfertaVentaId = ofertaInterna.OfertaVentaId,
                    ParMonedaId = parMonedaId,
                    CompradorId = orden.UsuarioId,
                    VendedorId = usuarioId,
                    CantidadEjecutada = cantidad,
                    PrecioUnitario = orden.PrecioUnitario,
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
                    UsuarioId = orden.UsuarioId,
                    MonedaId = par.MonedaDestinoId,
                    TipoMovimiento = "OrdenCompra",
                    Monto = cantidad,
                    SaldoAnterior = saldoAnteriorCompradorDestino,
                    SaldoPosterior = saldoCompradorDestino.SaldoDisponible,
                    FechaMovimiento = DateTime.Now,
                    ReferenciaTipo = "EjecucionOrden",
                    ReferenciaId = ejecucion.EjecucionId
                });

                await CrearNotificacionAsync(
                    orden.UsuarioId,
                    "Orden parcial",
                    "Orden ejecutada",
                    $"Tu orden recibió una ejecución por {cantidad} unidades.",
                    "EjecucionOrden",
                    ejecucion.EjecucionId);
            }

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Venta inmediata",
                ReferenciaId = operacion.OperacionInmediataId,
                ParMonedaId = parMonedaId,
                Estado = "Completada",
                MetodoEjecucion = metodoEjecucion,
                FechaHora = DateTime.Now
            });

            await CrearNotificacionAsync(
                usuarioId,
                metodoEjecucion == "Mejor ruta" ? "Mejor ruta" : "Venta inmediata",
                "Venta inmediata ejecutada",
                $"Tu venta inmediata fue ejecutada por un total de {totalRecibido}.",
                "OperacionInmediata",
                operacion.OperacionInmediataId);

            await _context.SaveChangesAsync();

            return await ConstruirRespuestaVentaAsync(operacion.OperacionInmediataId);
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

        private async Task<VentaInmediataResponseDto> ConstruirRespuestaVentaAsync(int operacionInmediataId)
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
                .Select(x => new DetalleEjecucionVentaDto
                {
                    EjecucionId = x.EjecucionId,
                    OrdenCompraId = x.Ejecucion.OrdenCompraId,
                    CompradorId = x.Ejecucion.CompradorId,
                    CantidadEjecutada = x.Ejecucion.CantidadEjecutada,
                    PrecioUnitario = x.Ejecucion.PrecioUnitario,
                    TotalOperacion = x.Ejecucion.TotalOperacion,
                    FechaEjecucion = x.Ejecucion.FechaEjecucion
                })
                .ToListAsync();

            return new VentaInmediataResponseDto
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
                TotalRecibido = operacion.TotalRecibido ?? 0,
                Estado = operacion.Estado,
                FechaOperacion = operacion.FechaOperacion,
                Ejecuciones = ejecuciones
            };
        }

        private async Task SincronizarOfertaEspejoDesdeOrdenAsync(
			OrdenesCompra orden,
			decimal cantidadComprada,
			decimal totalPagado)
		{
			var ofertaEspejo = await _context.OfertasVenta
				.FirstOrDefaultAsync(o => o.OrdenCompraEspejoId == orden.OrdenCompraId);

			if (ofertaEspejo == null)
				return;

			ofertaEspejo.CantidadOriginal = orden.TotalComprometido;
			ofertaEspejo.CantidadVendida = orden.TotalEjecutado;
			ofertaEspejo.CantidadPendiente = Math.Max(0, orden.TotalComprometido - orden.TotalEjecutado);
			ofertaEspejo.TotalEsperado = orden.CantidadOriginal;
			ofertaEspejo.TotalRecibido = orden.CantidadObtenida;
			ofertaEspejo.FechaActualizacion = DateTime.Now;
			ofertaEspejo.Estado = orden.Estado;
		}
    }
}