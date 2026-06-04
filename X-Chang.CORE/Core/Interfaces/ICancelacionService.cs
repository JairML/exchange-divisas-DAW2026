using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface ICancelacionService
    {
        // US-022: detalle para la ventana emergente de confirmación.
        Task<ResultadoOperacion<CancelacionDetalleDTO>> GetDetalle(int usuarioId, string tipoOperacion, int referenciaId);

        // US-022: confirma la cancelación de la orden u oferta.
        Task<ResultadoOperacion<CancelacionResultadoDTO>> Cancelar(int usuarioId, CancelacionConfirmarDTO dto);
    }
}
