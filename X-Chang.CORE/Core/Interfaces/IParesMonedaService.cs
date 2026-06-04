using X_Chang.CORE.Core.DTOs.Catalogos;

namespace X_Chang.CORE.Core.Interfaces;

public interface IParesMonedaService
{
    Task<List<ParMonedaDto>> ObtenerTodosAsync(bool? activo);
    Task<ParMonedaDetalleDto?> ObtenerDetalleAsync(int parMonedaId);
}
