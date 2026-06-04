using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Billetera;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class BilleteraConsultaService : IBilleteraConsultaService
{
    private readonly IBilleteraConsultaRepository _repo;
    private readonly ISesionUsuarioRepository _sesionRepo;
    private readonly IUsuarioRepository _usuarioRepo;

    public BilleteraConsultaService(
        IBilleteraConsultaRepository repo,
        ISesionUsuarioRepository sesionRepo,
        IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _sesionRepo = sesionRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<ResultadoOperacion<SaldoDetalleDto>> GetSaldoMonedaAsync(
        string tokenSesion, int monedaId)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<SaldoDetalleDto>.Error("Sesión inválida o expirada.");

        var saldo = await _repo.GetSaldoMonedaAsync(uid.Value, monedaId);
        if (saldo == null)
            return ResultadoOperacion<SaldoDetalleDto>.Error("Moneda no encontrada en la billetera.");

        return ResultadoOperacion<SaldoDetalleDto>.Ok(new SaldoDetalleDto
        {
            MonedaId = saldo.MonedaId,
            CodigoISO = saldo.Moneda.CodigoIso,
            NombreMoneda = saldo.Moneda.Nombre,
            SaldoDisponible = saldo.SaldoDisponible,
            FechaActualizacion = saldo.FechaActualizacion
        });
    }

    public async Task<ResultadoOperacion<MovimientosPaginadosDto>> GetMovimientosPaginadosAsync(
        string tokenSesion, int? monedaId, string? tipoMovimiento,
        DateTime? desde, DateTime? hasta, int pagina, int tamano)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<MovimientosPaginadosDto>.Error("Sesión inválida o expirada.");

        var (items, total) = await _repo.GetMovimientosPaginadosAsync(
            uid.Value, monedaId, tipoMovimiento, desde, hasta, pagina, tamano);

        return ResultadoOperacion<MovimientosPaginadosDto>.Ok(new MovimientosPaginadosDto
        {
            Movimientos = items.Select(m => new MovimientoDto
            {
                MovimientoId = m.MovimientoId,
                CodigoISO = m.Moneda.CodigoIso,
                NombreMoneda = m.Moneda.Nombre,
                TipoMovimiento = m.TipoMovimiento,
                Monto = m.Monto,
                SaldoAnterior = m.SaldoAnterior,
                SaldoPosterior = m.SaldoPosterior,
                FechaMovimiento = m.FechaMovimiento,
                ReferenciaTipo = m.ReferenciaTipo
            }).ToList(),
            TotalRegistros = total,
            Pagina = pagina,
            TamanoPagina = tamano
        });
    }

    private async Task<int?> ResolverUsuarioIdAsync(string tokenSesion)
    {
        if (string.IsNullOrWhiteSpace(tokenSesion)) return null;
        var sesion = await _sesionRepo.ObtenerSesionActivaAsync(tokenSesion);
        if (sesion == null) return null;
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(sesion.UsuarioId);
        return usuario?.Estado == "Activo" ? usuario.UsuarioId : null;
    }
}
