namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class HistorialTransaccionesRequestDto
    {
        // null = todas las columnas; valores válidos: OrdenesCompra, OfertasVenta,
        // ComprasInmediatas, VentasInmediatas, Depositos, Retiros
        public string? Columna { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int NumeroPagina { get; set; } = 1;
        // Opciones: 5, 10, 20, 40, 100, 200; 0 = Todos
        public int RegistrosPorPagina { get; set; } = 10;
    }
}
