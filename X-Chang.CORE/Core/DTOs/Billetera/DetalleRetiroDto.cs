namespace X_Chang.CORE.Core.DTOs.Billetera;

public class DetalleRetiroDto
{
    public int RetiroId { get; set; }
    public int MonedaId { get; set; }
    public string CodigoISO { get; set; } = null!;
    public string NombreMoneda { get; set; } = null!;
    public int MetodoPagoId { get; set; }
    public string NombreMetodoPago { get; set; } = null!;
    public decimal MontoRetirado { get; set; }
    public decimal ComisionAplicada { get; set; }
    public decimal MontoFinalRecibido { get; set; }
    public string Estado { get; set; } = null!;
    public string? VoucherUrl { get; set; }
    public DateTime FechaRetiro { get; set; }
}
