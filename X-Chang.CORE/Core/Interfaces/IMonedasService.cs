using X_Chang.CORE.Core.DTOs.Catalogos;

namespace X_Chang.CORE.Core.Interfaces;

public interface IMonedasService
{
    Task<List<MonedaDto>> ObtenerTodosAsync(string? tipo, bool? activa);
    Task<MonedaDto?> ObtenerPorIdAsync(int monedaId);
}
