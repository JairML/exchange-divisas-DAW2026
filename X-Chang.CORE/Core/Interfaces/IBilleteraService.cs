using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IBilleteraService
    {
        // US-006: resumen de la billetera del usuario (barra + lista completa).
        Task<BilleteraResumenDTO> GetBilletera(int usuarioId);
    }
}
