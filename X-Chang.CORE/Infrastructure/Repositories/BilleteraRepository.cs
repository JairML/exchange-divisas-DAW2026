using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X_Chang.API.Models;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Infrastructure.Repositories
{
    // US-006: acceso a datos de la billetera virtual y sus saldos por moneda.
    public class BilleteraRepository : IBilleteraRepository
    {
        private readonly ExchangeDivisasDbContext _context;

        public BilleteraRepository(ExchangeDivisasDbContext context)
        {
            _context = context;
        }

        public async Task<Billeteras?> GetBilleteraByUsuario(int usuarioId)
        {
            return await _context.Billeteras
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
        }

        public async Task<IEnumerable<SaldosBilletera>> GetSaldosByUsuario(int usuarioId)
        {
            // Se incluye la moneda para poder mostrar el código ISO y el nombre,
            // y se filtra por la billetera que pertenece al usuario autenticado.
            return await _context.SaldosBilletera
                .Include(s => s.Moneda)
                .Include(s => s.Billetera)
                .Where(s => s.Billetera.UsuarioId == usuarioId)
                .ToListAsync();
        }
    }
}
