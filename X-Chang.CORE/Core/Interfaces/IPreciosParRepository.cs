using X_Chang.CORE.Core.DTOs.Precios;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IPreciosParRepository
    {
        // Devuelve las monedas activas cuyo CodigoIso esté en la lista indicada
        Task<List<Monedas>> ObtenerMonedasSoportadasAsync(IEnumerable<string> codigosIso);

        // Devuelve todos los pares existentes en la BD (activos e inactivos)
        Task<List<ParesMoneda>> ObtenerTodosParesAsync();

        // Mayor PrecioUnitario de órdenes de compra activas, agrupado por ParMonedaId
        Task<Dictionary<int, decimal>> ObtenerMayoresPreciosCompraAsync();

        // Menor PrecioUnitario de ofertas de venta activas, agrupado por ParMonedaId
        Task<Dictionary<int, decimal>> ObtenerMenoresPreciosVentaAsync();

        // Suma de (VolumenCompra + VolumenVenta) del histórico, agrupado por ParMonedaId
        Task<Dictionary<int, decimal>> ObtenerVolumenesPorParAsync();

        // Fecha máxima de transacción por par en el historial de un usuario
        Task<Dictionary<int, DateTime>> ObtenerFechaTransaccionPorParUsuarioAsync(int usuarioId);

        // Serie histórica de precios de un par; `desde` null = todo el historial
        Task<List<PuntoSerieHistoricaDto>> ObtenerSerieHistoricaAsync(int parMonedaId, DateTime? desde);

        // CodigoIso de la moneda del país del usuario (vía Usuarios → Paises → Monedas)
        Task<string?> ObtenerMonedaPrincipalUsuarioAsync(int usuarioId);

        // ParMonedaId de la orden/oferta activa más reciente del usuario; null si no hay
        Task<int?> ObtenerParMasRecienteActivoUsuarioAsync(int usuarioId);

        // ParMonedaId para el par (origenId, destinoId); null si no existe en la BD
        Task<int?> ObtenerParMonedaIdAsync(int monedaOrigenId, int monedaDestinoId);

        // Códigos ISO (origen, destino) de un par dado su ParMonedaId
        Task<(string OrigenIso, string DestinoIso)?> ObtenerIsosPorParIdAsync(int parMonedaId);
    }
}
