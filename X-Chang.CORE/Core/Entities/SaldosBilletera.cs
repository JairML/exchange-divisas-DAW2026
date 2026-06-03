using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class SaldosBilletera
{
    public int SaldoId { get; set; }

    public int BilleteraId { get; set; }

    public int MonedaId { get; set; }

    public decimal SaldoDisponible { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Billeteras Billetera { get; set; } = null!;

    public virtual Monedas Moneda { get; set; } = null!;
}
