using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class HistorialTransacciones
{
    public int HistorialId { get; set; }

    public int UsuarioId { get; set; }

    public string TipoOperacion { get; set; } = null!;

    public int ReferenciaId { get; set; }

    public int? ParMonedaId { get; set; }

    public int? MonedaId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Estado { get; set; } = null!;

    public string? MetodoEjecucion { get; set; }

    public virtual Monedas? Moneda { get; set; }

    public virtual ParesMoneda? ParMoneda { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
