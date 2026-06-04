using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Precios;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IPreciosParService
    {
        // US-009: datos de los gráficos del menú principal
        Task<MenuPrincipalResponseDto> ObtenerDatosMenuPrincipalAsync(int? usuarioId);

        // US-010: listado paginado de pares con precios actuales
        Task<ParesMonedaPaginadoDto> ObtenerListadoParesAsync(int? usuarioId, FiltroParesMonedaDto filtro);

        // US-011: serie histórica de un par con indicadores actuales
        Task<ResultadoOperacion<SerieHistoricaParResponseDto>> ObtenerSerieHistoricaAsync(
            string monedaOrigen, string monedaDestino, string rango);
    }
}
