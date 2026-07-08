using System.ComponentModel.DataAnnotations.Schema;

namespace X_Chang.CORE.Core.Entities;

public partial class HistoricoPreciosPar
{
    public int HistoricoPrecioId { get; set; }

    public int ParMonedaId { get; set; }

    public decimal? MayorPrecioCompra { get; set; }

    public decimal? MenorPrecioVenta { get; set; }

    public decimal VolumenCompra { get; set; }

    public decimal VolumenVenta { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? SnapshotMinuto { get; set; }

    [NotMapped]
    public decimal? Margen =>
        MayorPrecioCompra.HasValue && MenorPrecioVenta.HasValue
            ? MenorPrecioVenta.Value - MayorPrecioCompra.Value
            : null;

    public virtual ParesMoneda ParMoneda { get; set; } = null!;
}