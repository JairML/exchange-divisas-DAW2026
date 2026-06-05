using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class BusquedasRuta
{
    public int BusquedaRutaId { get; set; }

    public int UsuarioId { get; set; }

    public int ParMonedaId { get; set; }

    public string TipoOperacion { get; set; } = null!;

    public decimal CantidadSolicitada { get; set; }

    public int MaxSaltos { get; set; }

    public int? TiempoEstimadoMs { get; set; }

    public int? TiempoRealMs { get; set; }

    public string Estado { get; set; } = null!;

    public decimal? TotalNormal { get; set; }

    public decimal? TotalRuta { get; set; }

    public decimal? AhorroEstimado { get; set; }

    public decimal? GananciaEstimada { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public virtual ParesMoneda ParMoneda { get; set; } = null!;

    public virtual ICollection<RutasConversion> RutasConversion { get; set; } = new List<RutasConversion>();

    public virtual Usuarios Usuario { get; set; } = null!;
}
