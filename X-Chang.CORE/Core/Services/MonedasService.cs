using X_Chang.CORE.Core.DTOs.Catalogos;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class MonedasService : IMonedasService
{
    private readonly IMonedasRepository _repo;
    public MonedasService(IMonedasRepository repo) => _repo = repo;

    public async Task<List<MonedaDto>> ObtenerTodosAsync(string? tipo, bool? activa)
    {
        var monedas = await _repo.ObtenerTodosAsync(tipo, activa);
        return monedas.Select(m => new MonedaDto
        {
            MonedaId = m.MonedaId,
            CodigoISO = m.CodigoIso,
            Nombre = m.Nombre,
            Tipo = m.Tipo,
            Activa = m.Activa
        }).ToList();
    }

    public async Task<MonedaDto?> ObtenerPorIdAsync(int monedaId)
    {
        var m = await _repo.ObtenerPorIdAsync(monedaId);
        if (m == null) return null;
        return new MonedaDto
        {
            MonedaId = m.MonedaId,
            CodigoISO = m.CodigoIso,
            Nombre = m.Nombre,
            Tipo = m.Tipo,
            Activa = m.Activa
        };
    }
}
