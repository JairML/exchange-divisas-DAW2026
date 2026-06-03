using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class MetodosPago
{
    public int MetodoPagoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public decimal ComisionPorcentaje { get; set; }

    public decimal ComisionFija { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Depositos> Depositos { get; set; } = new List<Depositos>();

    public virtual ICollection<MetodosPagoPais> MetodosPagoPais { get; set; } = new List<MetodosPagoPais>();

    public virtual ICollection<Retiros> Retiros { get; set; } = new List<Retiros>();
}
