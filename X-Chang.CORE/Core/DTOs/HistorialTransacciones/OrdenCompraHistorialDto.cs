namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class OrdenCompraHistorialDto
    {
        public int OrdenCompraId { get; set; }
        public DateTime FechaHora { get; set; }
        public string ParMonedas { get; set; } = string.Empty;
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadObtenida { get; set; }
        public decimal CantidadPendiente { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalComprometido { get; set; }
        public decimal TotalEjecutado { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
