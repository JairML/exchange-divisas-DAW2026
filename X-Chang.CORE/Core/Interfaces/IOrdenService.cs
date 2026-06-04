using X_Chang.CORE.Core.DTOs.Common;
using X_Chang.CORE.Core.DTOs.Ordenes;

namespace X_Chang.CORE.Core.Interfaces;

// US-004: consulta de órdenes de compra activas del usuario.
public interface IOrdenService
{
    Task<PagedResultDto<OrdenDto>> ObtenerMisOrdenesAsync(int usuarioId, FiltroOrdenesDto filtro);
    Task<OrdenDto> ObtenerOrdenAsync(int usuarioId, int ordenId);
}
