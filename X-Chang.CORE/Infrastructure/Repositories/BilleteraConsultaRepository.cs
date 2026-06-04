using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class BilleteraConsultaRepository : IBilleteraConsultaRepository
{
    private readonly ExchangeDivisasDbContext _context;
    public BilleteraConsultaRepository(ExchangeDivisasDbContext context) => _context = context;

    public async Task<SaldosBilletera?> GetSaldoMonedaAsync(int usuarioId, int monedaId)
    {
        var billeteraId = await _context.Billeteras
            .Where(b => b.UsuarioId == usuarioId)
            .Select(b => (int?)b.BilleteraId)
            .FirstOrDefaultAsync();

        if (billeteraId == null) return null;

        return await _context.SaldosBilletera
            .Include(s => s.Moneda)
            .FirstOrDefaultAsync(s => s.BilleteraId == billeteraId && s.MonedaId == monedaId);
    }

    public async Task<(List<MovimientosBilletera> items, int total)> GetMovimientosPaginadosAsync(
        int usuarioId, int? monedaId, string? tipoMovimiento,
        DateTime? desde, DateTime? hasta, int pagina, int tamano)
    {
        var query = _context.MovimientosBilletera
            .Include(m => m.Moneda)
            .Where(m => m.UsuarioId == usuarioId)
            .AsQueryable();

        if (monedaId.HasValue)
            query = query.Where(m => m.MonedaId == monedaId.Value);
        if (!string.IsNullOrWhiteSpace(tipoMovimiento))
            query = query.Where(m => m.TipoMovimiento == tipoMovimiento);
        if (desde.HasValue)
            query = query.Where(m => m.FechaMovimiento >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(m => m.FechaMovimiento <= hasta.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.FechaMovimiento)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        return (items, total);
    }
}
