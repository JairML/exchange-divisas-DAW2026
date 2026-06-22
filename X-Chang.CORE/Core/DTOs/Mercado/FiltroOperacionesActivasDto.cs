namespace X_Chang.CORE.Core.DTOs.Mercado
{
    public class FiltroOperacionesActivasDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Pagina { get; set; } = 1;
        public string RegistrosPorPagina { get; set; } = "10";
    }
}
