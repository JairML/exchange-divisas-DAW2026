using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface ICancelacionService
    {
        Task<ResultadoOperacion<CancelacionDetalleDTO>> GetDetalle(int usuarioId, string tipoOperacion, int referenciaId);
        Task<ResultadoOperacion<CancelacionResultadoDTO>> Cancelar(int usuarioId, CancelacionConfirmarDTO dto);
    }
}
