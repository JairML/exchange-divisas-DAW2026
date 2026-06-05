using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

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
            .FirstOrDefaultAsync(m => m.MetodoPagoId == metodoPagoId && m.Activo);
    }

    public async Task<IEnumerable<MetodosPago>> GetMetodosPagoDisponibles(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario == null)
            return Enumerable.Empty<MetodosPago>();

        return await _context.MetodosPago
            .Where(m => m.Activo && m.MetodosPagoPais.Any(mp => mp.PaisId == usuario.PaisId && mp.Activo))
            .ToListAsync();
    }

    public async Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

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
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var voucherUrl = $"vouchers/{Guid.NewGuid():N}.pdf";

            var deposito = new Depositos
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                MetodoPagoId = metodoPagoId,
                MontoDepositado = monto,
                ComisionAplicada = comision,
                TotalPagado = total,
                Estado = "Completada",
                VoucherUrl = voucherUrl,
                FechaDeposito = DateTime.UtcNow
            };
            _context.Depositos.Add(deposito);
            await _context.SaveChangesAsync();

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);

            if (billetera == null)
                throw new InvalidOperationException("Billetera no encontrada.");

            var saldo = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId
                                        && s.MonedaId == monedaId);

            decimal saldoAnterior;
            decimal saldoPosterior;

            if (saldo == null)
            {
                saldoAnterior = 0m;
                saldoPosterior = monto;
                var nuevoSaldo = new SaldosBilletera
                {
                    BilleteraId = billetera.BilleteraId,
                    MonedaId = monedaId,
                    SaldoDisponible = monto,
                    FechaActualizacion = DateTime.UtcNow
                };
                _context.SaldosBilletera.Add(nuevoSaldo);
            }
            else
            {
                saldoAnterior = saldo.SaldoDisponible;
                saldo.SaldoDisponible += monto;
                saldo.FechaActualizacion = DateTime.UtcNow;
                saldoPosterior = saldo.SaldoDisponible;
            }

            var movimiento = new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                TipoMovimiento = "Deposito",
                Monto = monto,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldoPosterior,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "Deposito",
                ReferenciaId = deposito.DepositoId
            };
            _context.MovimientosBilletera.Add(movimiento);

            var historial = new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Deposito",
                ReferenciaId = deposito.DepositoId,
                ParMonedaId = null,
                MonedaId = monedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = null
            };
            _context.HistorialTransacciones.Add(historial);

            var notificacion = new NotificacionesCorreo
            {
                UsuarioId = usuarioId,
                CorreoDestino = correoDestino,
                TipoEvento = "Deposito",
                Asunto = $"Deposito confirmado - {codigoIso}",
                Cuerpo = $"Su deposito de {monto} {codigoIso} ha sido registrado exitosamente. Total pagado: {total} {codigoIso}.",
                EstadoEnvio = "Pendiente",
                FechaCreacion = DateTime.UtcNow,
                ReferenciaTipo = "Deposito",
                ReferenciaId = deposito.DepositoId
            };
            _context.NotificacionesCorreo.Add(notificacion);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (deposito.DepositoId, saldoPosterior, deposito.FechaDeposito, voucherUrl);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
