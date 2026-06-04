using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface IPaisesRepository
{
    Task<List<Paises>> ObtenerTodosAsync();
    Task<Paises?> ObtenerPorIdAsync(int paisId);
}
