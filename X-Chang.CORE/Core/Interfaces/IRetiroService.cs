using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IRetiroService
    {
        // US-008: métodos de cobro disponibles para el usuario según su país.
        Task<IEnumerable<MetodoPagoDTO>> GetMetodosCobro(int usuarioId);

        // US-008: calcula comisión y monto final a recibir (resumen previo a confirmar).
        Task<ResultadoOperacion<RetiroResumenDTO>> Calcular(int usuarioId, RetiroCalcularDTO dto);

        // US-008: confirma y registra el retiro.
        Task<ResultadoOperacion<RetiroResultadoDTO>> RegistrarRetiro(int usuarioId, RetiroCreateDTO dto);
    }
}
