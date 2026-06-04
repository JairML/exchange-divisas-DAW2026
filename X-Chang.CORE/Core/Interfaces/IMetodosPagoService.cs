using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Catalogos;

namespace X_Chang.CORE.Core.Interfaces;

public interface IMetodosPagoService
{
    Task<List<MetodoPagoDto>> ObtenerTodosAsync();
    Task<MetodoPagoDto?> ObtenerPorIdAsync(int metodoPagoId);
    Task<ResultadoOperacion<List<MetodoPagoDto>>> ObtenerParaDepositoAsync(string tokenSesion);
    Task<ResultadoOperacion<List<MetodoPagoDto>>> ObtenerParaRetiroAsync(string tokenSesion);
}
