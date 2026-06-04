using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Catalogos;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class MetodosPagoRepository : IMetodosPagoRepository
{
    private readonly ExchangeDivisasDbContext _context;
    public MetodosPagoRepository(ExchangeDivisasDbContext context) => _context = context;

    public Task<List<MetodoPagoDto>> ObtenerTodosActivosAsync() =>
        _context.MetodosPago
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new MetodoPagoDto
            {
                MetodoPagoId = m.MetodoPagoId,
                Nombre = m.Nombre,
                Tipo = m.Tipo,
                ComisionPorcentaje = m.ComisionPorcentaje,
                ComisionFija = m.ComisionFija
            })
            .ToListAsync();

    public async Task<MetodoPagoDto?> ObtenerPorIdAsync(int metodoPagoId)
    {
        var m = await _context.MetodosPago.FirstOrDefaultAsync(x => x.MetodoPagoId == metodoPagoId);
        if (m == null) return null;
        return new MetodoPagoDto
        {
            MetodoPagoId = m.MetodoPagoId,
            Nombre = m.Nombre,
            Tipo = m.Tipo,
            ComisionPorcentaje = m.ComisionPorcentaje,
            ComisionFija = m.ComisionFija
        };
    }

    public Task<List<MetodoPagoDto>> ObtenerParaPaisAsync(int paisId, string[] tipos) =>
        _context.MetodosPagoPais
            .Where(mpp => mpp.PaisId == paisId
                && mpp.Activo
                && mpp.MetodoPago.Activo
                && tipos.Contains(mpp.MetodoPago.Tipo))
            .OrderBy(mpp => mpp.MetodoPago.Nombre)
            .Select(mpp => new MetodoPagoDto
            {
                MetodoPagoId = mpp.MetodoPago.MetodoPagoId,
                Nombre = mpp.MetodoPago.Nombre,
                Tipo = mpp.MetodoPago.Tipo,
                ComisionPorcentaje = mpp.MetodoPago.ComisionPorcentaje,
                ComisionFija = mpp.MetodoPago.ComisionFija
            })
            .ToListAsync();
}
