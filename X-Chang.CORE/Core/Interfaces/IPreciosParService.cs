using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Precios;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IPreciosParService
    {
        Task<MenuPrincipalResponseDto> ObtenerDatosMenuPrincipalAsync(int? usuarioId);
        Task<ParesMonedaPaginadoDto> ObtenerListadoParesAsync(int? usuarioId, FiltroParesMonedaDto filtro);
        Task<ResultadoOperacion<SerieHistoricaParResponseDto>> ObtenerSerieHistoricaAsync(
            string monedaOrigen, string monedaDestino, string rango);
    }
}
