using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class RestriccionesUsuario
{
    public int RestriccionId { get; set; }

    public int UsuarioId { get; set; }

    public int AdministradorId { get; set; }

    public string TipoAccion { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string EstadoRestriccion { get; set; } = null!;

    public virtual Usuarios Administrador { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
