namespace X_Chang.CORE.Core.DTOs.Ordenes;

// US-004: representación de una orden de compra en la vista de transacciones activas.
public record OrdenDto(
    int OrdenCompraId,
    int ParMonedaId,
    string MonedaOrigen,
    string MonedaDestino,
    decimal CantidadOriginal,
    decimal CantidadObtenida,
    decimal CantidadRestante,
    decimal PrecioUnitario,
    decimal TotalOriginal,
    decimal TotalRestante,
    string Estado,
    DateTime FechaCreacion);

public record FiltroOrdenesDto(
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10);
