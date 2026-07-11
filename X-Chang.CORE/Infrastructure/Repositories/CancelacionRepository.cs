using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;
using X_Chang.CORE.Infrastructure.Shared;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    // US-022: acceso a datos de la cancelación de orden u oferta.
    public class CancelacionRepository : ICancelacionRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        // Estados en los que una orden u oferta todavía puede cancelarse.
        private static readonly string[] EstadosCancelables =
            { "Activa", "Parcialmente ejecutada" };

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
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
                .FirstOrDefaultAsync(o => o.OrdenCompraId == ordenCompraId);
        }

        public async Task<OfertasVenta?> GetOferta(int ofertaVentaId)
        {
            return await _context.OfertasVenta
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaOrigen)
                .Include(o => o.ParMoneda).ThenInclude(p => p.MonedaDestino)
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
            var ahora = DateTime.Now;
            var esOrden = tipoOperacion == "Orden de compra";

            using var tx = await _context.Database.BeginTransactionAsync();

            int? ordenCompraId = null;
            int? ofertaVentaId = null;

            if (esOrden)
            {
                var orden = await _context.OrdenesCompra
                    .FirstOrDefaultAsync(o =>
                        o.OrdenCompraId == referenciaId &&
                        o.UsuarioId == usuarioId);

                if (orden == null || !EstadosCancelables.Contains(orden.Estado))
                {
                    await tx.RollbackAsync();
                    return null;
                }

                var estadoAnteriorOrden = orden.Estado;
                orden.Estado = "Cancelada";
                orden.FechaCancelacion = ahora;
                orden.FechaActualizacion = ahora;
                ordenCompraId = orden.OrdenCompraId;
                _context.LogEstadosOperacion.Add(new LogEstadosOperacion
                {
                    TipoOperacion = "OrdenCompra",
                    ReferenciaId = orden.OrdenCompraId,
                    EstadoAnterior = estadoAnteriorOrden,
                    EstadoNuevo = "Cancelada",
                    FechaCambio = ahora,
                    Motivo = "Cancelación"
                });

                var ofertaEspejo = await _context.OfertasVenta
                    .FirstOrDefaultAsync(o => o.OrdenCompraEspejoId == orden.OrdenCompraId);

                if (ofertaEspejo != null &&
                    EstadosCancelables.Contains(ofertaEspejo.Estado))
                {
                    ofertaEspejo.Estado = "Cancelada";
                    ofertaEspejo.FechaCancelacion = ahora;
                    ofertaEspejo.FechaActualizacion = ahora;
                }
            }
            else
            {
                var oferta = await _context.OfertasVenta
                    .FirstOrDefaultAsync(o =>
                        o.OfertaVentaId == referenciaId &&
                        o.UsuarioId == usuarioId);

                if (oferta == null || !EstadosCancelables.Contains(oferta.Estado))
                {
                    await tx.RollbackAsync();
                    return null;
                }

                var estadoAnteriorOferta = oferta.Estado;
                oferta.Estado = "Cancelada";
                oferta.FechaCancelacion = ahora;
                oferta.FechaActualizacion = ahora;
                ofertaVentaId = oferta.OfertaVentaId;
                _context.LogEstadosOperacion.Add(new LogEstadosOperacion
                {
                    TipoOperacion = "OfertaVenta",
                    ReferenciaId = oferta.OfertaVentaId,
                    EstadoAnterior = estadoAnteriorOferta,
                    EstadoNuevo = "Cancelada",
                    FechaCambio = ahora,
                    Motivo = "Cancelación"
                });

                if (oferta.OrdenCompraEspejoId != null)
                {
                    var ordenEspejo = await _context.OrdenesCompra
                        .FirstOrDefaultAsync(o =>
                            o.OrdenCompraId == oferta.OrdenCompraEspejoId.Value);

                    if (ordenEspejo != null &&
                        EstadosCancelables.Contains(ordenEspejo.Estado))
                    {
                        ordenEspejo.Estado = "Cancelada";
                        ordenEspejo.FechaCancelacion = ahora;
                        ordenEspejo.FechaActualizacion = ahora;
                    }
                }
            }

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
                FechaCancelacion = ahora
            };

            _context.CancelacionesOrdenOferta.Add(cancelacion);
            await _context.SaveChangesAsync();

            decimal nuevoSaldo;

            if (montoReembolsado > 0m)
            {
                var (_, posterior) = await MovimientoBilleteraHelper.Aplicar(
                    _context,
                    usuarioId,
                    monedaReembolsoId,
                    montoReembolsado,
                    "Reembolso",
                    "Cancelacion",
                    cancelacion.CancelacionId);

                nuevoSaldo = posterior;
            }
            else
            {
                nuevoSaldo = await _context.SaldosBilletera
                    .Where(s =>
                        s.Billetera.UsuarioId == usuarioId &&
                        s.MonedaId == monedaReembolsoId)
                    .Select(s => s.SaldoDisponible)
                    .FirstOrDefaultAsync();
            }

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Cancelacion",
                ReferenciaId = cancelacion.CancelacionId,
                ParMonedaId = parMonedaId,
                MonedaId = null,
                FechaHora = ahora,
                Estado = "Cancelada",
                MetodoEjecucion = null
            });

            var tipoNotificacionId = await _context.TiposNotificacion
                .Where(t => t.Nombre == "Cancelación")
                .Select(t => (int?)t.TipoNotificacionId)
                .FirstOrDefaultAsync();

            _context.NotificacionesCorreo.Add(new NotificacionesCorreo
            {
                UsuarioId = usuarioId,
                CorreoDestino = correoDestino,
                TipoEvento = "Cancelación",
                TipoNotificacionId = tipoNotificacionId,
                Asunto = "Cancelación realizada",
                Cuerpo = $"Tu {tipoOperacion.ToLower()} fue cancelada. Monto reembolsado: {montoReembolsado}.",
                EstadoEnvio = "Pendiente",
                FechaCreacion = ahora,
                ReferenciaTipo = "Cancelacion",
                ReferenciaId = cancelacion.CancelacionId
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (cancelacion.CancelacionId, nuevoSaldo, ahora);
        }
    }
}
