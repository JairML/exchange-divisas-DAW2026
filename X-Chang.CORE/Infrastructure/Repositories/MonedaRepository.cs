using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    public class MonedaRepository : IMonedaRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public MonedaRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Monedas>> GetMonedasActivas()
        {
            return await _context.Monedas
                .Where(m => m.Activa)
                .OrderBy(m => m.CodigoIso)
                .ToListAsync();
        }
    }
}
