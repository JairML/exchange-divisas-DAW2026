namespace X_Chang.CORE.Core.DTOs.Billetera;

public class MovimientoDto
{
    public int MovimientoId { get; set; }
    public string CodigoISO { get; set; } = null!;
    public string NombreMoneda { get; set; } = null!;
    public string TipoMovimiento { get; set; } = null!;
    public decimal Monto { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoPosterior { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string? ReferenciaTipo { get; set; }
}
