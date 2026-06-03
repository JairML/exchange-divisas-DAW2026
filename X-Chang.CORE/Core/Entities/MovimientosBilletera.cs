using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class MovimientosBilletera
{
    public int MovimientoId { get; set; }

    public int UsuarioId { get; set; }

    public int MonedaId { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Monto { get; set; }

    public decimal SaldoAnterior { get; set; }

    public decimal SaldoPosterior { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? ReferenciaTipo { get; set; }

    public int? ReferenciaId { get; set; }

    public virtual Monedas Moneda { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
