using X_Chang.CORE.Core.DTOs.Catalogos;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class ParesMonedaService : IParesMonedaService
{
    private readonly IParesMonedaRepository _repo;
    public ParesMonedaService(IParesMonedaRepository repo) => _repo = repo;

    public Task<List<ParMonedaDto>> ObtenerTodosAsync(bool? activo) =>
        _repo.ObtenerTodosAsync(activo);

    public Task<ParMonedaDetalleDto?> ObtenerDetalleAsync(int parMonedaId) =>
        _repo.ObtenerDetalleAsync(parMonedaId);
}
