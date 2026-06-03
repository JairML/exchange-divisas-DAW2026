using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class AdjuntosCorreo
{
    public int AdjuntoId { get; set; }

    public int NotificacionId { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string UrlArchivo { get; set; } = null!;

    public string? TipoContenido { get; set; }

    public virtual NotificacionesCorreo Notificacion { get; set; } = null!;
}
