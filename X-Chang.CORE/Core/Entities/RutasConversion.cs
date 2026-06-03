using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class RutasConversion
{
    public int RutaConversionId { get; set; }

    public int BusquedaRutaId { get; set; }

    public int? OperacionInmediataId { get; set; }

    public int MonedaInicialId { get; set; }

    public int MonedaFinalId { get; set; }

    public int CantidadSaltos { get; set; }

    public decimal TotalEstimado { get; set; }

    public decimal? AhorroEstimado { get; set; }

    public decimal? GananciaEstimada { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual BusquedasRuta BusquedaRuta { get; set; } = null!;

    public virtual Monedas MonedaFinal { get; set; } = null!;

    public virtual Monedas MonedaInicial { get; set; } = null!;

    public virtual OperacionesInmediatas? OperacionInmediata { get; set; }

    public virtual ICollection<RutaConversionSaltos> RutaConversionSaltos { get; set; } = new List<RutaConversionSaltos>();
}
