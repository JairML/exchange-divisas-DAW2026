using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class ExportacionesReporte
{
    public int ExportacionId { get; set; }

    public int UsuarioId { get; set; }

    public string TipoReporte { get; set; } = null!;

    public string Formato { get; set; } = null!;

    public DateTime? FechaDesde { get; set; }

    public DateTime? FechaHasta { get; set; }

    public string? UrlArchivo { get; set; }

    public DateTime FechaExportacion { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
