namespace X_Chang.CORE.Core.DTOs.Billetera;

public class MovimientosPaginadosDto
{
    public List<MovimientoDto> Movimientos { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
}
