using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Shared;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    // US-007: acceso a datos del depósito de dinero a la billetera.
    public class DepositoRepository : IDepositoRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public DepositoRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> GetUsuario(int usuarioId)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        }

        public async Task<Monedas?> GetMoneda(int monedaId)
        {
            return await _context.Monedas
                .FirstOrDefaultAsync(m => m.MonedaId == monedaId);
        }

        public async Task<MetodosPago?> GetMetodoPago(int metodoPagoId)
        {
            return await _context.MetodosPago
                .FirstOrDefaultAsync(mp => mp.MetodoPagoId == metodoPagoId);
        }

        // Métodos de pago habilitados para DEPÓSITO en el país del usuario.
        // Un método sirve para depositar si su Tipo es 'Pago' o 'Ambos'.
        // La disponibilidad por país es data-driven (tabla MetodosPagoPais);
        // p. ej. Yape solo está habilitado en Perú.
        public async Task<IEnumerable<MetodosPago>> GetMetodosPagoDisponibles(int usuarioId)
        {
            var paisId = await _context.Usuarios
                .Where(u => u.UsuarioId == usuarioId)
                .Select(u => u.PaisId)
                .FirstOrDefaultAsync();

            if (paisId == 0)
                return new List<MetodosPago>();

            return await _context.MetodosPagoPais
                .Where(mpp => mpp.PaisId == paisId
                              && mpp.Activo
                              && mpp.MetodoPago.Activo
                              && (mpp.MetodoPago.Tipo == "Pago" || mpp.MetodoPago.Tipo == "Ambos"))
                .Select(mpp => mpp.MetodoPago)
                .Distinct()
                .OrderBy(mp => mp.Nombre)
                .ToListAsync();
        }

        public async Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId)
        {
            var paisId = await _context.Usuarios
                .Where(u => u.UsuarioId == usuarioId)
                .Select(u => u.PaisId)
                .FirstOrDefaultAsync();

            if (paisId == 0)
                return false;

            return await _context.MetodosPagoPais
                .AnyAsync(mpp => mpp.PaisId == paisId
                                 && mpp.MetodoPagoId == metodoPagoId
                                 && mpp.Activo
                                 && mpp.MetodoPago.Activo
                                 && (mpp.MetodoPago.Tipo == "Pago" || mpp.MetodoPago.Tipo == "Ambos"));
        }

        public async Task<string?> GetConfiguracion(string clave)
        {
            return await _context.ConfiguracionSistema
                .Where(c => c.Clave == clave)
                .Select(c => c.Valor)
                .FirstOrDefaultAsync();
        }

        // Registra el depósito de forma atómica. Toda la operación va dentro de una
        // transacción para que el depósito, el saldo, el movimiento, el historial y la
        // notificación se persistan juntos o no se persista nada.
        public async Task<(int depositoId, decimal nuevoSaldo, DateTime fecha, string voucherUrl)> RegistrarDeposito(
            int usuarioId,
            int monedaId,
            int metodoPagoId,
            decimal monto,
            decimal comision,
            decimal total,
            string correoDestino,
            string codigoIso)
        {
            var ahora = DateTime.Now;

            using var tx = await _context.Database.BeginTransactionAsync();

            // 1) Crear el depósito (estado Completada según los criterios de aceptación).
            var deposito = new Depositos
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                MetodoPagoId = metodoPagoId,
                MontoDepositado = monto,
                ComisionAplicada = comision,
                TotalPagado = total,
                Estado = "Completada",
                FechaDeposito = ahora
            };
            _context.Depositos.Add(deposito);
            await _context.SaveChangesAsync(); // obtiene el DepositoId generado

            // 2) Voucher (US-018 enviará el correo; aquí solo dejamos la URL del comprobante).
            var voucherUrl = $"https://x-chang.local/vouchers/deposito-{deposito.DepositoId}.pdf";
            deposito.VoucherUrl = voucherUrl;

            // 3) Abonar el monto depositado al saldo de la moneda.
            var (_, posterior) = await MovimientoBilleteraHelper.Aplicar(
                _context, usuarioId, monedaId, monto, "Deposito", "Deposito", deposito.DepositoId);

            // 4) Registrar en el historial de transacciones.
            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Deposito",
                ReferenciaId = deposito.DepositoId,
                ParMonedaId = null,
                MonedaId = monedaId,
                FechaHora = ahora,
                Estado = "Completada",
                MetodoEjecucion = null
            });

            // 5) Encolar la notificación de correo (el envío real es responsabilidad de US-018).
            var tipoNotificacionId = await _context.TiposNotificacion
                .Where(t => t.Nombre == "Depósito")
                .Select(t => (int?)t.TipoNotificacionId)
                .FirstOrDefaultAsync();

            var notificacion = new NotificacionesCorreo
            {
                UsuarioId = usuarioId,
                CorreoDestino = correoDestino,
                TipoEvento = "Depósito",
                TipoNotificacionId = tipoNotificacionId,
                Asunto = "Depósito completado",
                Cuerpo = $"Tu depósito de {monto} {codigoIso} fue completado. Total pagado: {total} {codigoIso}.",
                EstadoEnvio = "Pendiente",
                FechaCreacion = ahora,
                ReferenciaTipo = "Deposito",
                ReferenciaId = deposito.DepositoId
            };
            _context.NotificacionesCorreo.Add(notificacion);
            await _context.SaveChangesAsync(); // obtiene el NotificacionId generado

            // 6) Adjuntar el voucher a la notificación.
            _context.AdjuntosCorreo.Add(new AdjuntosCorreo
            {
                NotificacionId = notificacion.NotificacionId,
                NombreArchivo = $"voucher-deposito-{deposito.DepositoId}.pdf",
                UrlArchivo = voucherUrl,
                TipoContenido = "application/pdf"
            });
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            return (deposito.DepositoId, posterior, ahora, voucherUrl);
        }
    }
}
