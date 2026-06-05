using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class CancelacionRepository : ICancelacionRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public CancelacionRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<Usuarios?> GetUsuario(int usuarioId)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
    }

    public async Task<OrdenesCompra?> GetOrden(int ordenCompraId)
    {
        return await _context.OrdenesCompra
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenCompraId);
    }

    public async Task<OfertasVenta?> GetOferta(int ofertaVentaId)
    {
        return await _context.OfertasVenta
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaOrigen)
            .Include(o => o.ParMoneda)
                .ThenInclude(p => p.MonedaDestino)
            .FirstOrDefaultAsync(o => o.OfertaVentaId == ofertaVentaId);
    }

    public async Task<(int cancelacionId, decimal nuevoSaldo, DateTime fecha)?> EjecutarCancelacion(
        string tipoOperacion,
        int referenciaId,
        int usuarioId,
        int parMonedaId,
        int monedaReembolsoId,
        decimal montoReembolsado,
        decimal cantidadEjecutada,
        decimal cantidadCancelada,
        string correoDestino)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            string estadoActual;
            int? ordenCompraId = null;
            int? ofertaVentaId = null;

            if (tipoOperacion == "Orden de compra")
            {
                var orden = await _context.OrdenesCompra
                    .FirstOrDefaultAsync(o => o.OrdenCompraId == referenciaId);

                if (orden == null) return null;

                estadoActual = orden.Estado;
                if (estadoActual != "Activa" && estadoActual != "Parcialmente ejecutada")
                    return null;

                orden.Estado = "Cancelada";
                orden.FechaCancelacion = DateTime.UtcNow;
                orden.FechaActualizacion = DateTime.UtcNow;
                ordenCompraId = referenciaId;
            }
            else
            {
                var oferta = await _context.OfertasVenta
                    .FirstOrDefaultAsync(o => o.OfertaVentaId == referenciaId);

                if (oferta == null) return null;

                estadoActual = oferta.Estado;
                if (estadoActual != "Activa" && estadoActual != "Parcialmente ejecutada")
                    return null;

                oferta.Estado = "Cancelada";
                oferta.FechaCancelacion = DateTime.UtcNow;
                oferta.FechaActualizacion = DateTime.UtcNow;
                ofertaVentaId = referenciaId;
            }

            // Registrar la cancelacion
            var cancelacion = new CancelacionesOrdenOferta
            {
                UsuarioId = usuarioId,
                TipoOperacion = tipoOperacion,
                OrdenCompraId = ordenCompraId,
                OfertaVentaId = ofertaVentaId,
                ParMonedaId = parMonedaId,
                CantidadEjecutada = cantidadEjecutada,
                CantidadCancelada = cantidadCancelada,
                MontoReembolsado = montoReembolsado,
                FechaCancelacion = DateTime.UtcNow
            };
            _context.CancelacionesOrdenOferta.Add(cancelacion);
            await _context.SaveChangesAsync();

            // Reembolsar el saldo en la billetera del usuario
            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
                throw new InvalidOperationException("Billetera no encontrada.");

            var saldo = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId
                                        && s.MonedaId == monedaReembolsoId);

            decimal saldoAnterior;
            decimal saldoPosterior;

            if (saldo == null)
            {
                saldoAnterior = 0m;
                saldoPosterior = montoReembolsado;
                var nuevoSaldo = new SaldosBilletera
                {
                    BilleteraId = billetera.BilleteraId,
                    MonedaId = monedaReembolsoId,
                    SaldoDisponible = montoReembolsado,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.SaldosBilletera.Add(nuevoSaldo);
            }
            else
            {
                saldoAnterior = saldo.SaldoDisponible;
                saldo.SaldoDisponible += montoReembolsado;
                saldo.FechaActualizacion = DateTime.UtcNow;
                saldoPosterior = saldo.SaldoDisponible;
            }

            // Registrar movimiento de billetera
            var movimiento = new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = monedaReembolsoId,
                TipoMovimiento = "Reembolso cancelacion",
                Monto = montoReembolsado,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldoPosterior,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = tipoOperacion,
                ReferenciaId = cancelacion.CancelacionId
            };
            _context.MovimientosBilletera.Add(movimiento);

            // Registrar historial de transacciones
            var historial = new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Cancelacion",
                ReferenciaId = cancelacion.CancelacionId,
                ParMonedaId = parMonedaId,
                MonedaId = monedaReembolsoId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = null
            };
            _context.HistorialTransacciones.Add(historial);

            // Registrar notificacion
            var notificacion = new NotificacionesCorreo
            {
                UsuarioId = usuarioId,
                CorreoDestino = correoDestino,
                TipoEvento = "Cancelacion",
                Asunto = $"Cancelacion de {tipoOperacion}",
                Cuerpo = $"Su {tipoOperacion} ha sido cancelada. Monto reembolsado: {montoReembolsado}.",
                EstadoEnvio = "Pendiente",
                FechaCreacion = DateTime.UtcNow,
                ReferenciaTipo = tipoOperacion,
                ReferenciaId = cancelacion.CancelacionId
            };
            _context.NotificacionesCorreo.Add(notificacion);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (cancelacion.CancelacionId, saldoPosterior, cancelacion.FechaCancelacion);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
