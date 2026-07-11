namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class ExportarHistorialRequestDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Columna { get; set; }
    }
}
