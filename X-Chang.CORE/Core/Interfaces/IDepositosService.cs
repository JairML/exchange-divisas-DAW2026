using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Billetera;

namespace X_Chang.CORE.Core.Interfaces;

public interface IDepositosService
{
    Task<ResultadoOperacion<(List<DetalleDepositoDto> items, int total)>> ListarAsync(
        string tokenSesion, int pagina, int tamano);
    Task<ResultadoOperacion<DetalleDepositoDto>> ObtenerDetalleAsync(
        string tokenSesion, int depositoId);
    Task<ResultadoOperacion<DetalleDepositoDto>> DepositarAsync(
        string tokenSesion, int monedaId, int metodoPagoId, decimal monto);
}
