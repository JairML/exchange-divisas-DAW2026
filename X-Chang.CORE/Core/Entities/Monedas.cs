using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class Monedas
{
    public int MonedaId { get; set; }

    public string CodigoIso { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public bool Activa { get; set; }

    public virtual ICollection<Depositos> Depositos { get; set; } = new List<Depositos>();

    public virtual ICollection<HistorialTransacciones> HistorialTransacciones { get; set; } = new List<HistorialTransacciones>();

    public virtual ICollection<MovimientosBilletera> MovimientosBilletera { get; set; } = new List<MovimientosBilletera>();

    public virtual ICollection<Paises> Paises { get; set; } = new List<Paises>();

    public virtual ICollection<ParesMoneda> ParesMonedaMonedaDestino { get; set; } = new List<ParesMoneda>();

    public virtual ICollection<ParesMoneda> ParesMonedaMonedaOrigen { get; set; } = new List<ParesMoneda>();

    public virtual ICollection<Retiros> Retiros { get; set; } = new List<Retiros>();

    public virtual ICollection<RutaConversionSaltos> RutaConversionSaltosMonedaDestino { get; set; } = new List<RutaConversionSaltos>();

    public virtual ICollection<RutaConversionSaltos> RutaConversionSaltosMonedaOrigen { get; set; } = new List<RutaConversionSaltos>();

    public virtual ICollection<RutasConversion> RutasConversionMonedaFinal { get; set; } = new List<RutasConversion>();

    public virtual ICollection<RutasConversion> RutasConversionMonedaInicial { get; set; } = new List<RutasConversion>();

    public virtual ICollection<SaldosBilletera> SaldosBilletera { get; set; } = new List<SaldosBilletera>();
}
