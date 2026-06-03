using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class OperacionesInmediatas
{
    public int OperacionInmediataId { get; set; }

    public int UsuarioId { get; set; }

    public int ParMonedaId { get; set; }

    public string TipoOperacion { get; set; } = null!;

    public string MetodoEjecucion { get; set; } = null!;

    public decimal CantidadSolicitada { get; set; }

    public decimal CantidadEjecutada { get; set; }

    public decimal? PrecioMinimo { get; set; }

    public decimal? PrecioMaximo { get; set; }

    public decimal? PrecioPromedio { get; set; }

    public decimal? TotalPagado { get; set; }

    public decimal? TotalRecibido { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaOperacion { get; set; }

    public int? OperacionPadreId { get; set; }

    public virtual ICollection<OperacionesInmediatas> InverseOperacionPadre { get; set; } = new List<OperacionesInmediatas>();

    public virtual ICollection<OperacionInmediataEjecuciones> OperacionInmediataEjecuciones { get; set; } = new List<OperacionInmediataEjecuciones>();

    public virtual OperacionesInmediatas? OperacionPadre { get; set; }

    public virtual ParesMoneda ParMoneda { get; set; } = null!;

    public virtual ICollection<RutaConversionSaltos> RutaConversionSaltosOperacionInmediata { get; set; } = new List<RutaConversionSaltos>();

    public virtual ICollection<RutaConversionSaltos> RutaConversionSaltosOperacionInmediataHija { get; set; } = new List<RutaConversionSaltos>();

    public virtual ICollection<RutasConversion> RutasConversion { get; set; } = new List<RutasConversion>();

    public virtual Usuarios Usuario { get; set; } = null!;
}
