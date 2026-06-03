using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class EjecucionesOrden
{
    public int EjecucionId { get; set; }

    public int OrdenCompraId { get; set; }

    public int OfertaVentaId { get; set; }

    public int ParMonedaId { get; set; }

    public int CompradorId { get; set; }

    public int VendedorId { get; set; }

    public decimal CantidadEjecutada { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal TotalOperacion { get; set; }

    public DateTime FechaEjecucion { get; set; }

    public virtual Usuarios Comprador { get; set; } = null!;

    public virtual OfertasVenta OfertaVenta { get; set; } = null!;

    public virtual ICollection<OperacionInmediataEjecuciones> OperacionInmediataEjecuciones { get; set; } = new List<OperacionInmediataEjecuciones>();

    public virtual OrdenesCompra OrdenCompra { get; set; } = null!;

    public virtual ParesMoneda ParMoneda { get; set; } = null!;

    public virtual Usuarios Vendedor { get; set; } = null!;
}
