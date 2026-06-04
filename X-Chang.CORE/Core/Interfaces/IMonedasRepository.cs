using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface IMonedasRepository
{
    Task<List<Monedas>> ObtenerTodosAsync(string? tipo, bool? activa);
    Task<Monedas?> ObtenerPorIdAsync(int monedaId);
}
