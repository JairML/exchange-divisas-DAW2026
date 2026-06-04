using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class TiposNotificacion
{
    public int TipoNotificacionId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<NotificacionesCorreo> NotificacionesCorreo { get; set; } = new List<NotificacionesCorreo>();
}
