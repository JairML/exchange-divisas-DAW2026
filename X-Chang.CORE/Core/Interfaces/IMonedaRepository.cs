using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.API.Models;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IMonedaRepository
    {
        Task<IEnumerable<Monedas>> GetMonedasActivas();
    }
}
