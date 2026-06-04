using X_Chang.CORE.Core.DTOs.Common;
using X_Chang.CORE.Core.DTOs.Ofertas;

namespace X_Chang.CORE.Core.Interfaces;

// US-004: consulta de ofertas de venta activas del usuario.
public interface IOfertaService
{
    Task<PagedResultDto<OfertaDto>> ObtenerMisOfertasAsync(int usuarioId, FiltroOfertasDto filtro);
    Task<OfertaDto> ObtenerOfertaAsync(int usuarioId, int ofertaId);
}
