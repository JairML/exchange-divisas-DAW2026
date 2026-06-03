using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IDepositoService
    {
        // US-007: métodos de pago disponibles para el usuario.
        Task<IEnumerable<MetodoPagoDTO>> GetMetodosPago(int usuarioId);

        // US-007: calcula comisión y total a pagar (resumen previo a confirmar).
        Task<ResultadoOperacion<DepositoResumenDTO>> Calcular(int usuarioId, DepositoCalcularDTO dto);

        // US-007: confirma y registra el depósito.
        Task<ResultadoOperacion<DepositoResultadoDTO>> RegistrarDeposito(int usuarioId, DepositoCreateDTO dto);
    }
}
