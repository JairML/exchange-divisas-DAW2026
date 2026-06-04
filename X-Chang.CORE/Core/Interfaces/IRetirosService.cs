using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Billetera;

namespace X_Chang.CORE.Core.Interfaces;

public interface IRetirosService
{
    Task<ResultadoOperacion<(List<DetalleRetiroDto> items, int total)>> ListarAsync(
        string tokenSesion, int pagina, int tamano);
    Task<ResultadoOperacion<DetalleRetiroDto>> ObtenerDetalleAsync(
        string tokenSesion, int retiroId);
    Task<ResultadoOperacion<DetalleRetiroDto>> RetirarAsync(
        string tokenSesion, RetirarDto dto);
}
