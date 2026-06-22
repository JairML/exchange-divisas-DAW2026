namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class OperacionActivaDto
    {
        public int Id { get; set; }
        public string TipoOperacion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int ParMonedaId { get; set; }
        public string Par { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadEjecutada { get; set; }
        public decimal CantidadRestante { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal TotalEjecutado { get; set; }
        public decimal TotalRestante { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool PuedeCancelar { get; set; }
    }
}
