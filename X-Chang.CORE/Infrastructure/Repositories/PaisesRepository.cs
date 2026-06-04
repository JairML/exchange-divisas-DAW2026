using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class PaisesRepository : IPaisesRepository
{
    private readonly ExchangeDivisasDbContext _context;
    public PaisesRepository(ExchangeDivisasDbContext context) => _context = context;

    public Task<List<Paises>> ObtenerTodosAsync() =>
        _context.Paises
            .Include(p => p.Moneda)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

    public Task<Paises?> ObtenerPorIdAsync(int paisId) =>
        _context.Paises
            .Include(p => p.Moneda)
            .FirstOrDefaultAsync(p => p.PaisId == paisId);
}
