namespace X_Chang.CORE.Core.DTOs.HistorialTransacciones
{
    public class PaginadoDto<T>
    {
        public int TotalRegistros { get; set; }
        public int NumeroPagina { get; set; }
        public int RegistrosPorPagina { get; set; }
        public int TotalPaginas { get; set; }
        public List<T> Lista { get; set; } = new();
    }
}
