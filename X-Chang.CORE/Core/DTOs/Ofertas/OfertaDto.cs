namespace X_Chang.CORE.Core.DTOs.Ofertas;

// US-004: representación de una oferta de venta en la vista de transacciones activas.
public record OfertaDto(
    int OfertaVentaId,
    int ParMonedaId,
    string MonedaOrigen,
    string MonedaDestino,
    decimal CantidadOriginal,
    decimal CantidadVendida,
    decimal CantidadRestante,
    decimal PrecioUnitario,
    decimal TotalOriginal,
    decimal TotalRestante,
    string Estado,
    DateTime FechaCreacion);

public record FiltroOfertasDto(
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10);
