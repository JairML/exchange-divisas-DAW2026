namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class ParesMonedaPaginadoDto
    {
        public List<ParMonedaListadoDto> Registros { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string RegistrosPorPagina { get; set; } = "20";
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }
}
