namespace X_Chang.CORE.Core.DTOs.Billetera;

public class SaldoDetalleDto
{
    public int MonedaId { get; set; }
    public string CodigoISO { get; set; } = null!;
    public string NombreMoneda { get; set; } = null!;
    public decimal SaldoDisponible { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
