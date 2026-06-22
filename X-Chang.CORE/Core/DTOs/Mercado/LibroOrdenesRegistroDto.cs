namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class LibroOrdenesRegistroDto
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
