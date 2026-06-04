namespace X_Chang.CORE.Core.DTOs.Catalogos;

public class MonedaDto
{
    public int MonedaId { get; set; }
    public string CodigoISO { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public bool Activa { get; set; }
}
