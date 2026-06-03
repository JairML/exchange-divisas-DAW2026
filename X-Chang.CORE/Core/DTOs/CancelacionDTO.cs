using System;

namespace X_Chang.CORE.Core.DTOs
{
    // Detalle mostrado en la ventana emergente antes de confirmar la cancelación (US-022).
    public class CancelacionDetalleDTO
    {
        // "Orden de compra" | "Oferta de venta"
        public string TipoOperacion { get; set; } = string.Empty;
        public int ReferenciaId { get; set; }
        public string Par { get; set; } = string.Empty; // ej. "PEN/USD"
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadEjecutada { get; set; }
        public decimal CantidadPendiente { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal MontoReembolso { get; set; }
        public string MonedaReembolso { get; set; } = string.Empty; // código ISO en que se reembolsa
        public string Estado { get; set; } = string.Empty;
        public bool PuedeCancelar { get; set; }
    }

    // Entrada para confirmar la cancelación.
    public class CancelacionConfirmarDTO
    {
        // "Orden de compra" | "Oferta de venta"
        public string TipoOperacion { get; set; } = string.Empty;
        // OrdenCompraId u OfertaVentaId según el tipo.
        public int ReferenciaId { get; set; }
    }

    // Resultado de la cancelación ejecutada.
    public class CancelacionResultadoDTO
    {
        public int CancelacionId { get; set; }
        public string TipoOperacion { get; set; } = string.Empty;
        public string Par { get; set; } = string.Empty;
        public decimal CantidadEjecutada { get; set; }
        public decimal CantidadCancelada { get; set; }
        public decimal MontoReembolsado { get; set; }
        public string MonedaReembolso { get; set; } = string.Empty;
        public decimal NuevoSaldo { get; set; }
        public string Estado { get; set; } = "Cancelada";
        public DateTime FechaCancelacion { get; set; }
    }
}
