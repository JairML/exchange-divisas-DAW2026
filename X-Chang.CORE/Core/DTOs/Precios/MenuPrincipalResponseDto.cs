namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class MenuPrincipalResponseDto
    {
        public bool UsuarioAutenticado { get; set; }
        public GraficoPreciosParDto GraficoPrincipal { get; set; } = new();

        // null cuando el usuario no está autenticado (solo se devuelve un gráfico)
        public GraficoPreciosParDto? GraficoSecundario { get; set; }
    }
}
