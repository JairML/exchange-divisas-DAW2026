using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Billetera;

namespace X_Chang.CORE.Core.Interfaces;

public interface IBilleteraConsultaService
{
    Task<ResultadoOperacion<SaldoDetalleDto>> GetSaldoMonedaAsync(string tokenSesion, int monedaId);
    Task<ResultadoOperacion<MovimientosPaginadosDto>> GetMovimientosPaginadosAsync(
        string tokenSesion, int? monedaId, string? tipoMovimiento,
        DateTime? desde, DateTime? hasta, int pagina, int tamano);
}
