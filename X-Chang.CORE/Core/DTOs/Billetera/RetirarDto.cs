namespace X_Chang.CORE.Core.DTOs.Billetera;

public class RetirarDto
{
    public int MonedaId { get; set; }
    public int MetodoPagoId { get; set; }
    public decimal MontoRetirado { get; set; }
    public string? VoucherUrl { get; set; }
}
