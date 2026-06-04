using System;
using System.Collections.Generic;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.API.Models;

public partial class SesionesUsuario
{
    public int SesionId { get; set; }

    public int UsuarioId { get; set; }

    public string TokenSesion { get; set; } = null!;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public DateTime? FechaCierre { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
