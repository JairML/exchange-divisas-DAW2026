using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IBilleteraRepository
    {
        Task<Billeteras?> GetBilleteraByUsuario(int usuarioId);
        // Devuelve los saldos del usuario incluyendo la moneda relacionada.
        Task<IEnumerable<SaldosBilletera>> GetSaldosByUsuario(int usuarioId);
    }
}
