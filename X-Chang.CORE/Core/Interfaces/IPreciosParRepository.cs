using X_Chang.CORE.Core.DTOs.Precios;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IPreciosParRepository
    {
        Task<List<Monedas>> ObtenerMonedasSoportadasAsync(IEnumerable<string>? codigosIso = null);

        Task<List<ParesMoneda>> ObtenerTodosParesAsync();

        Task<int?> ObtenerParMonedaIdAsync(int monedaOrigenId, int monedaDestinoId);

        Task<(string OrigenIso, string DestinoIso)?> ObtenerIsosPorParIdAsync(int parMonedaId);

        Task<string?> ObtenerMonedaPrincipalUsuarioAsync(int usuarioId);

        Task<int?> ObtenerParMasRecienteActivoUsuarioAsync(int usuarioId);

        Task<Dictionary<int, decimal>> ObtenerMayoresPreciosCompraAsync();

        Task<Dictionary<int, decimal>> ObtenerMenoresPreciosVentaAsync();

        Task<Dictionary<int, decimal>> ObtenerVolumenesPorParAsync();

        Task<Dictionary<int, DateTime>> ObtenerFechaTransaccionPorParUsuarioAsync(int usuarioId);

        Task<List<PuntoSerieHistoricaDto>> ObtenerSerieHistoricaAsync(
            int parMonedaId,
            DateTime? desde);
    }
}