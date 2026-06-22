using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IBilleteraService
    {
        Task<BilleteraResumenDTO> GetBilletera(int usuarioId);
    }
}
