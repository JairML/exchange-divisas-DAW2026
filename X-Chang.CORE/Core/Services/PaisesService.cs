using X_Chang.CORE.Core.DTOs.Catalogos;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class PaisesService : IPaisesService
{
    private readonly IPaisesRepository _repo;
    public PaisesService(IPaisesRepository repo) => _repo = repo;

    public async Task<List<PaisDto>> ObtenerTodosAsync()
    {
        var paises = await _repo.ObtenerTodosAsync();
        return paises.Select(MapPais).ToList();
    }

    public async Task<PaisDto?> ObtenerPorIdAsync(int paisId)
    {
        var p = await _repo.ObtenerPorIdAsync(paisId);
        return p == null ? null : MapPais(p);
    }

    private static PaisDto MapPais(Paises p) => new()
    {
        PaisId = p.PaisId,
        Nombre = p.Nombre,
        Moneda = new MonedaDto
        {
            MonedaId = p.Moneda.MonedaId,
            CodigoISO = p.Moneda.CodigoIso,
            Nombre = p.Moneda.Nombre,
            Tipo = p.Moneda.Tipo,
            Activa = p.Moneda.Activa
        }
    };
}
