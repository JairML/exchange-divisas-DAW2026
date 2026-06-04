using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    // US-008: acceso a datos para el retiro de dinero de la billetera.
    public class RetiroRepository : IRetiroRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public RetiroRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<Usuarios?> GetUsuario(int usuarioId)
        {
            return await _context.Usuarios.FindAsync(usuarioId);
        }

        public async Task<Monedas?> GetMoneda(int monedaId)
        {
            return await _context.Monedas.FindAsync(monedaId);
        }

        public async Task<MetodosPago?> GetMetodoPago(int metodoPagoId)
        {
            return await _context.MetodosPago.FindAsync(metodoPagoId);
        }

        public async Task<IEnumerable<MetodosPago>> GetMetodosCobroDisponibles(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                return Array.Empty<MetodosPago>();

            return await _context.MetodosPago
                .Where(m => m.Activo &&
                            m.MetodosPagoPais.Any(mp => mp.PaisId == usuario.PaisId && mp.Activo))
                .ToListAsync();
        }

        public async Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                return false;

            return await _context.MetodosPagoPais
                .AnyAsync(mp => mp.MetodoPagoId == metodoPagoId
                             && mp.PaisId == usuario.PaisId
                             && mp.Activo);
        }

        public async Task<string?> GetConfiguracion(string clave)
        {
            var config = await _context.ConfiguracionSistema
                .FirstOrDefaultAsync(c => c.Clave == clave);
            return config?.Valor;
        }

        public async Task<decimal> GetSaldoDisponible(int usuarioId, int monedaId)
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

        public async Task<(int retiroId, decimal nuevoSaldo, DateTime fecha, string voucherUrl)> RegistrarRetiro(
            int usuarioId,
            int monedaId,
            int metodoPagoId,
            decimal montoRetirado,
            decimal comision,
            decimal montoFinalRecibido,
            string correoDestino,
            string codigoIso)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fechaAhora = DateTime.Now;

                // 1. Crear el registro de retiro.
                var retiro = new Retiros
                {
                    UsuarioId = usuarioId,
                    MonedaId = monedaId,
                    MetodoPagoId = metodoPagoId,
                    MontoRetirado = montoRetirado,
                    ComisionAplicada = comision,
                    MontoFinalRecibido = montoFinalRecibido,
                    Estado = "Completada",
                    FechaRetiro = fechaAhora
                };
                _context.Retiros.Add(retiro);
                await _context.SaveChangesAsync();

                // 2. Descontar el monto completo del saldo de billetera.
                var billetera = await _context.Billeteras
                    .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

                var saldo = await _context.SaldosBilletera
                    .FirstOrDefaultAsync(s => s.BilleteraId == billetera!.BilleteraId
                                           && s.MonedaId == monedaId);

                var saldoAnterior = saldo!.SaldoDisponible;
                saldo.SaldoDisponible -= montoRetirado;
                saldo.FechaActualizacion = fechaAhora;

                // 3. Registrar el movimiento de billetera (débito).
                _context.MovimientosBilletera.Add(new MovimientosBilletera
                {
                    UsuarioId = usuarioId,
                    MonedaId = monedaId,
                    TipoMovimiento = "Debito",
                    Monto = montoRetirado,
                    SaldoAnterior = saldoAnterior,
                    SaldoPosterior = saldo.SaldoDisponible,
                    FechaMovimiento = fechaAhora,
                    ReferenciaTipo = "Retiro",
                    ReferenciaId = retiro.RetiroId
                });

                // 4. Registrar en el historial de transacciones.
                _context.HistorialTransacciones.Add(new HistorialTransacciones
                {
                    UsuarioId = usuarioId,
                    TipoOperacion = "Retiro",
                    ReferenciaId = retiro.RetiroId,
                    MonedaId = monedaId,
                    FechaHora = fechaAhora,
                    Estado = "Completada"
                });

                // 5. Guardar URL del voucher en el registro de retiro.
                var voucherUrl = $"vouchers/retiros/{retiro.RetiroId}";
                retiro.VoucherUrl = voucherUrl;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (retiro.RetiroId, saldo.SaldoDisponible, fechaAhora, voucherUrl);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
