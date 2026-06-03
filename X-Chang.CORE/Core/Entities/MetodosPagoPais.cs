using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class MetodosPagoPais
{
    public int MetodoPagoPaisId { get; set; }

    public int MetodoPagoId { get; set; }

    public int PaisId { get; set; }

    public bool Activo { get; set; }

    public virtual MetodosPago MetodoPago { get; set; } = null!;

    public virtual Paises Pais { get; set; } = null!;
}
