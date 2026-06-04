namespace X_Chang.CORE.Core.DTOs.Catalogos;

public class MetodoPagoDto
{
    public int MetodoPagoId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public decimal ComisionPorcentaje { get; set; }
    public decimal ComisionFija { get; set; }
}
