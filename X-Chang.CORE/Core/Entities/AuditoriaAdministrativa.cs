using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class AuditoriaAdministrativa
{
    public int AuditoriaId { get; set; }

    public int AdministradorId { get; set; }

    public int UsuarioAfectadoId { get; set; }

    public string TipoAccion { get; set; } = null!;

    public string MensajeRegistrado { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public virtual Usuarios Administrador { get; set; } = null!;

    public virtual Usuarios UsuarioAfectado { get; set; } = null!;
}
