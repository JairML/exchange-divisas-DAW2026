using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IDepositoService
    {
        Task<IEnumerable<MetodoPagoDTO>> GetMetodosPago(int usuarioId);
        Task<ResultadoOperacion<DepositoResumenDTO>> Calcular(int usuarioId, DepositoCalcularDTO dto);
        Task<ResultadoOperacion<DepositoResultadoDTO>> RegistrarDeposito(int usuarioId, DepositoCreateDTO dto);
    }
}
