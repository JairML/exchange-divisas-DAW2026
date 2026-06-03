using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class Retiros
{
    public int RetiroId { get; set; }

    public int UsuarioId { get; set; }

    public int MonedaId { get; set; }

    public int MetodoPagoId { get; set; }

    public decimal MontoRetirado { get; set; }

    public decimal ComisionAplicada { get; set; }

    public decimal MontoFinalRecibido { get; set; }

    public string Estado { get; set; } = null!;

    public string? VoucherUrl { get; set; }

    public DateTime FechaRetiro { get; set; }

    public virtual MetodosPago MetodoPago { get; set; } = null!;

    public virtual Monedas Moneda { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
