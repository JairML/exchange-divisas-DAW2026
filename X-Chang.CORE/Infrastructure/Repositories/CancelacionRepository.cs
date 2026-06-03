using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.Core.Interfaces;
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

            // 1) Volver a leer la operación DENTRO de la transacción y revalidar su estado.
            //    Esto evita la condición de carrera: si entre la confirmación y la ejecución
            //    la operación ya se completó o se canceló, no se debe volver a cancelar.
            int? ordenCompraId = null;
            int? ofertaVentaId = null;

            if (esOrden)
            {
                var orden = await _context.OrdenesCompra
                    .FirstOrDefaultAsync(o => o.OrdenCompraId == referenciaId && o.UsuarioId == usuarioId);
                if (orden == null || !EstadosCancelables.Contains(orden.Estado))
                {
                    await tx.RollbackAsync();
                    return null;
                }
                orden.Estado = "Cancelada";
                orden.FechaCancelacion = ahora;
                orden.FechaActualizacion = ahora;
                ordenCompraId = orden.OrdenCompraId;
            }
            else
            {
                var oferta = await _context.OfertasVenta
                    .FirstOrDefaultAsync(o => o.OfertaVentaId == referenciaId && o.UsuarioId == usuarioId);
                if (oferta == null || !EstadosCancelables.Contains(oferta.Estado))
                {
                    await tx.RollbackAsync();
                    return null;
                }
                oferta.Estado = "Cancelada";
                oferta.FechaCancelacion = ahora;
                oferta.FechaActualizacion = ahora;
                ofertaVentaId = oferta.OfertaVentaId;
            }

            // 2) Registrar la cancelación (CHECK de BD: exactamente una referencia no nula).
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
            await _context.SaveChangesAsync(); // obtiene el CancelacionId generado

            // 3) Reembolsar el saldo comprometido que no llegó a ejecutarse.
            decimal nuevoSaldo;
            if (montoReembolsado > 0m)
            {
                var (_, posterior) = await MovimientoBilleteraHelper.Aplicar(
                    _context, usuarioId, monedaReembolsoId, montoReembolsado,
                    "Reembolso", "Cancelacion", cancelacion.CancelacionId);
                nuevoSaldo = posterior;
            }
            else
            {
                // Sin reembolso: se devuelve el saldo actual de la moneda (0 si no existe).
                nuevoSaldo = await _context.SaldosBilletera
                    .Where(s => s.Billetera.UsuarioId == usuarioId && s.MonedaId == monedaReembolsoId)
                    .Select(s => s.SaldoDisponible)
                    .FirstOrDefaultAsync();
            }

            // 4) Registrar en el historial de transacciones.
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

            // 5) Encolar la notificación de correo (el envío real es responsabilidad de US-018).
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
