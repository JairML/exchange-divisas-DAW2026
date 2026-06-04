using Microsoft.EntityFrameworkCore;
using X_Chang.CORE.Core.DTOs.Billetera;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Infrastructure.Data;

namespace X_Chang.CORE.Infrastructure.Repositories;

public class DepositosRepository : IDepositosRepository
{
    private readonly ExchangeDivisasDbContext _context;
    public DepositosRepository(ExchangeDivisasDbContext context) => _context = context;

    private static DetalleDepositoDto Map(Core.Entities.Depositos d) => new()
    {
        DepositoId = d.DepositoId,
        MonedaId = d.MonedaId,
        CodigoISO = d.Moneda.CodigoIso,
        NombreMoneda = d.Moneda.Nombre,
        MetodoPagoId = d.MetodoPagoId,
        NombreMetodoPago = d.MetodoPago.Nombre,
        MontoDepositado = d.MontoDepositado,
        ComisionAplicada = d.ComisionAplicada,
        TotalPagado = d.TotalPagado,
        Estado = d.Estado,
        VoucherUrl = d.VoucherUrl,
        FechaDeposito = d.FechaDeposito
    };

    public async Task<List<DetalleDepositoDto>> ListarAsync(int usuarioId, int pagina, int tamano) =>
        (await _context.Depositos
            .Include(d => d.Moneda)
            .Include(d => d.MetodoPago)
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.FechaDeposito)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync())
            .Select(Map).ToList();

    public Task<int> ContarAsync(int usuarioId) =>
        _context.Depositos.CountAsync(d => d.UsuarioId == usuarioId);

    public async Task<DetalleDepositoDto?> ObtenerDetalleAsync(int usuarioId, int depositoId)
    {
        var d = await _context.Depositos
            .Include(d => d.Moneda)
            .Include(d => d.MetodoPago)
            .FirstOrDefaultAsync(d => d.DepositoId == depositoId && d.UsuarioId == usuarioId);

        return d == null ? null : Map(d);
    }
}
