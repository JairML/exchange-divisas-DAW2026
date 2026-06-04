using X_Chang.CORE.Core.DTOs.Catalogos;

namespace X_Chang.CORE.Core.Interfaces;

public interface IPaisesService
{
    Task<List<PaisDto>> ObtenerTodosAsync();
    Task<PaisDto?> ObtenerPorIdAsync(int paisId);
}
