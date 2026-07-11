using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class RetiroRepository : IRetiroRepository
{
    private readonly ExchangeDivisasDbContext _context;

    public RetiroRepository(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<Usuarios?> GetUsuario(int usuarioId)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

    public async Task<Monedas?> GetMoneda(int monedaId)
        => await _context.Monedas.FirstOrDefaultAsync(m => m.MonedaId == monedaId);

    public async Task<MetodosPago?> GetMetodoPago(int metodoPagoId)
        => await _context.MetodosPago.FirstOrDefaultAsync(m => m.MetodoPagoId == metodoPagoId && m.Activo);

    public async Task<IEnumerable<MetodosPago>> GetMetodosPagoDisponibles(int usuarioId)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        if (usuario == null) return Enumerable.Empty<MetodosPago>();

        return await _context.MetodosPago
            .Where(m => m.Activo && m.MetodosPagoPais.Any(mp => mp.PaisId == usuario.PaisId && mp.Activo))
            .ToListAsync();
    }

    public async Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);
        if (usuario == null) return false;

        return await _context.MetodosPagoPais
            .AnyAsync(mp => mp.MetodoPagoId == metodoPagoId && mp.PaisId == usuario.PaisId && mp.Activo);
    }

    public async Task<string?> GetConfiguracion(string clave)
    {
        var config = await _context.ConfiguracionSistema.FirstOrDefaultAsync(c => c.Clave == clave);
        return config?.Valor;
    }

    public async Task<decimal> GetSaldoDisponible(int usuarioId, int monedaId)
    {
        var billetera = await _context.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
        if (billetera == null) return 0m;

        var saldo = await _context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId && s.MonedaId == monedaId);

        return saldo?.SaldoDisponible ?? 0m;
    }

    public async Task<(int retiroId, decimal nuevoSaldo, DateTime fecha, string? voucherUrl)> RegistrarRetiro(
        int usuarioId, int monedaId, int metodoPagoId,
        decimal monto, decimal comision, decimal montoFinal,
        string correoDestino, string codigoIso)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var voucherUrl = $"vouchers/{Guid.NewGuid():N}.pdf";

            var retiro = new Retiros
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                MetodoPagoId = metodoPagoId,
                MontoRetirado = monto,
                ComisionAplicada = comision,
                MontoFinalRecibido = montoFinal,
                Estado = "Completada",
                VoucherUrl = voucherUrl,
                FechaRetiro = DateTime.UtcNow
            };
            _context.Retiros.Add(retiro);
            await _context.SaveChangesAsync();

            var billetera = await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
                ?? throw new InvalidOperationException("Billetera no encontrada.");

            var saldo = await _context.SaldosBilletera
                .FirstOrDefaultAsync(s => s.BilleteraId == billetera.BilleteraId && s.MonedaId == monedaId)
                ?? throw new InvalidOperationException("Saldo no encontrado.");

            var saldoAnterior = saldo.SaldoDisponible;
            saldo.SaldoDisponible -= monto;
            saldo.FechaActualizacion = DateTime.UtcNow;
            var saldoPosterior = saldo.SaldoDisponible;

            _context.MovimientosBilletera.Add(new MovimientosBilletera
            {
                UsuarioId = usuarioId,
                MonedaId = monedaId,
                TipoMovimiento = "Retiro",
                Monto = -monto,
                SaldoAnterior = saldoAnterior,
                SaldoPosterior = saldoPosterior,
                FechaMovimiento = DateTime.UtcNow,
                ReferenciaTipo = "Retiro",
                ReferenciaId = retiro.RetiroId
            });

            _context.HistorialTransacciones.Add(new HistorialTransacciones
            {
                UsuarioId = usuarioId,
                TipoOperacion = "Retiro",
                ReferenciaId = retiro.RetiroId,
                ParMonedaId = null,
                MonedaId = monedaId,
                FechaHora = DateTime.UtcNow,
                Estado = "Completada",
                MetodoEjecucion = null
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (retiro.RetiroId, saldoPosterior, retiro.FechaRetiro, voucherUrl);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
