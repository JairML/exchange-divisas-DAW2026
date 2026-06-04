using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface IBilleteraConsultaRepository
{
    Task<SaldosBilletera?> GetSaldoMonedaAsync(int usuarioId, int monedaId);
    Task<(List<MovimientosBilletera> items, int total)> GetMovimientosPaginadosAsync(
        int usuarioId, int? monedaId, string? tipoMovimiento,
        DateTime? desde, DateTime? hasta, int pagina, int tamano);
}
