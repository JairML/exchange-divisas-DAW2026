using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class AccesosUsuario
{
    public int AccesoId { get; set; }

    public int UsuarioId { get; set; }

    public DateTime FechaAcceso { get; set; }

    public bool Exitoso { get; set; }

    public string MetodoIngreso { get; set; } = null!;

    public string? MensajeResultado { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}

