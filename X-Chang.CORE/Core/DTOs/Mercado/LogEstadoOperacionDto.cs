namespace X_Chang.CORE.Core.DTOs.Mercado;

public class LogEstadoOperacionDto
{
    public int LogId { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = null!;
    public DateTime FechaCambio { get; set; }
    public string? Motivo { get; set; }
    public decimal? CantidadAfectada { get; set; }
}
