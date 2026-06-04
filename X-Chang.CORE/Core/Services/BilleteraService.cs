using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Services;

public class BilleteraService
{
    private readonly ExchangeDivisasDbContext _context;

    public BilleteraService(ExchangeDivisasDbContext context)
    {
        _context = context;
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