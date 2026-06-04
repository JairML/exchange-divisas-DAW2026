namespace X_Chang.CORE.Core.DTOs.Catalogos;

public class PaisDto
{
    public int PaisId { get; set; }
    public string Nombre { get; set; } = null!;
    public MonedaDto Moneda { get; set; } = null!;
}
