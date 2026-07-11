using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Services;

public class BilleteraService : IBilleteraService
{
    private readonly ExchangeDivisasDbContext _context;

    public BilleteraService(ExchangeDivisasDbContext context)
    {
        _context = context;
    }

    public async Task<BilleteraResumenDTO> GetBilletera(int usuarioId)
    {
        var billetera = await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
            ?? throw new InvalidOperationException("Billetera no encontrada.");

        var saldos = await _context.Monedas
            .Where(m => m.Activa)
            .GroupJoin(
                _context.SaldosBilletera.Where(s => s.BilleteraId == billetera.BilleteraId),
                m => m.MonedaId,
                s => s.MonedaId,
                (m, saldosMoneda) => new { Moneda = m, SaldosMoneda = saldosMoneda })
            .SelectMany(
                x => x.SaldosMoneda.DefaultIfEmpty(),
                (x, saldo) => new SaldoMonedaDTO
                {
                    MonedaId = x.Moneda.MonedaId,
                    CodigoISO = x.Moneda.CodigoIso,
                    Nombre = x.Moneda.Nombre,
                    SaldoDisponible = saldo != null ? saldo.SaldoDisponible : 0
                })
            .OrderByDescending(s => s.SaldoDisponible)
            .ToListAsync();

        return new BilleteraResumenDTO
        {
            UsuarioId = usuarioId,
            BilleteraId = billetera.BilleteraId,
            TieneFondos = saldos.Any(s => s.SaldoDisponible > 0),
            Saldos = saldos,
            SaldosConFondos = saldos.Where(s => s.SaldoDisponible > 0).ToList()
        };
    }

    public static async Task<SaldosBilletera> ObtenerOCrearSaldoInternoAsync(
        ExchangeDivisasDbContext context, int billeteraId, int monedaId)
    {
        var saldo = await context.SaldosBilletera
            .FirstOrDefaultAsync(s => s.BilleteraId == billeteraId && s.MonedaId == monedaId);

        if (saldo != null) return saldo;

        saldo = new SaldosBilletera
        {
            BilleteraId = billeteraId,
            MonedaId = monedaId,
            SaldoDisponible = 0,
            FechaActualizacion = DateTime.UtcNow
        };
        context.SaldosBilletera.Add(saldo);
        return saldo;
    }

    private Task<SaldosBilletera> ObtenerOCrearSaldoAsync(int billeteraId, int monedaId)
        => ObtenerOCrearSaldoInternoAsync(_context, billeteraId, monedaId);
}