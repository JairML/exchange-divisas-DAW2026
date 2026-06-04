using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class MonedasRepository : IMonedasRepository
{
    private readonly ExchangeDivisasDbContext _context;
    public MonedasRepository(ExchangeDivisasDbContext context) => _context = context;

    public async Task<List<Monedas>> ObtenerTodosAsync(string? tipo, bool? activa)
    {
        var query = _context.Monedas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(m => m.Tipo == tipo);

        if (activa.HasValue)
            query = query.Where(m => m.Activa == activa.Value);

        return await query.OrderBy(m => m.CodigoIso).ToListAsync();
    }

    public Task<Monedas?> ObtenerPorIdAsync(int monedaId) =>
        _context.Monedas.FirstOrDefaultAsync(m => m.MonedaId == monedaId);
}
