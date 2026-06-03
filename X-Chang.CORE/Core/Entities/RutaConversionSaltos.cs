using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class RutaConversionSaltos
{
    public int RutaConversionSaltoId { get; set; }

    public int RutaConversionId { get; set; }

    public int NumeroSalto { get; set; }

    public int ParMonedaId { get; set; }

    public int MonedaOrigenId { get; set; }

    public int MonedaDestinoId { get; set; }

    public decimal CantidadConvertida { get; set; }

    public decimal? PrecioMinimo { get; set; }

    public decimal? PrecioMaximo { get; set; }

    public decimal? PrecioPromedio { get; set; }

    public decimal ResultadoObtenido { get; set; }

    public int? OperacionInmediataId { get; set; }

    public int? OperacionInmediataHijaId { get; set; }

    public virtual Monedas MonedaDestino { get; set; } = null!;

    public virtual Monedas MonedaOrigen { get; set; } = null!;

    public virtual OperacionesInmediatas? OperacionInmediata { get; set; }

    public virtual OperacionesInmediatas? OperacionInmediataHija { get; set; }

    public virtual ParesMoneda ParMoneda { get; set; } = null!;

    public virtual RutasConversion RutaConversion { get; set; } = null!;
}
