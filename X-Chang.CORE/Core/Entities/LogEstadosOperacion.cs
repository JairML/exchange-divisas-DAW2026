namespace X_Chang.CORE.Core.Entities;

public class LogEstadosOperacion
{
    public int LogId { get; set; }
    public string TipoOperacion { get; set; } = null!;
    public int ReferenciaId { get; set; }
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = null!;
    public DateTime FechaCambio { get; set; }
    public string? Motivo { get; set; }
    public decimal? CantidadAfectada { get; set; }
}
