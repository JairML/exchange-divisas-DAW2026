using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class CancelacionesOrdenOferta
{
    public int CancelacionId { get; set; }

    public int UsuarioId { get; set; }

    public string TipoOperacion { get; set; } = null!;

    public int? OrdenCompraId { get; set; }

    public int? OfertaVentaId { get; set; }

    public int ParMonedaId { get; set; }

    public decimal CantidadEjecutada { get; set; }

    public decimal CantidadCancelada { get; set; }

    public decimal MontoReembolsado { get; set; }

    public DateTime FechaCancelacion { get; set; }

    public virtual OfertasVenta? OfertaVenta { get; set; }

    public virtual OrdenesCompra? OrdenCompra { get; set; }

    public virtual ParesMoneda ParMoneda { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
