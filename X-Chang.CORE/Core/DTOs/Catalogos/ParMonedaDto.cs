namespace X_Chang.CORE.Core.DTOs.Catalogos;

public class ParMonedaDto
{
    public int ParMonedaId { get; set; }
    public MonedaDto MonedaOrigen { get; set; } = null!;
    public MonedaDto MonedaDestino { get; set; } = null!;
    public bool Activo { get; set; }
    public decimal? MejorPrecioCompra { get; set; }
    public decimal? MejorPrecioVenta { get; set; }
    public decimal? Spread { get; set; }
}
