using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class NotificacionesCorreo
{
    public int NotificacionId { get; set; }

    public int UsuarioId { get; set; }

    public string CorreoDestino { get; set; } = null!;

    public string TipoEvento { get; set; } = null!;

    public string Asunto { get; set; } = null!;

    public string Cuerpo { get; set; } = null!;

    public string EstadoEnvio { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? ReferenciaTipo { get; set; }

    public int? ReferenciaId { get; set; }

    public int? TipoNotificacionId { get; set; }

    public virtual ICollection<AdjuntosCorreo> AdjuntosCorreo { get; set; } = new List<AdjuntosCorreo>();

    public virtual TiposNotificacion? TipoNotificacion { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
