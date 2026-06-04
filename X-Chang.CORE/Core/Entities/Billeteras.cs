using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class Billeteras
{
    public int BilleteraId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<SaldosBilletera> SaldosBilletera { get; set; } = new List<SaldosBilletera>();

    public virtual Usuarios Usuario { get; set; } = null!;
}
