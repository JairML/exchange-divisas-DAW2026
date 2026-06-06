using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IRetiroService
    {
        Task<IEnumerable<MetodoPagoDTO>> GetMetodosCobro(int usuarioId);
        Task<ResultadoOperacion<RetiroResumenDTO>> Calcular(int usuarioId, RetiroCalcularDTO dto);
        Task<ResultadoOperacion<RetiroResultadoDTO>> RegistrarRetiro(int usuarioId, RetiroCreateDTO dto);
    }
}
