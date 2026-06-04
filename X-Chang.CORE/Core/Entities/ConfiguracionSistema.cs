using System;
using System.Collections.Generic;

namespace X_Chang.CORE.Core.Entities;

public partial class ConfiguracionSistema
{
    public int ConfiguracionId { get; set; }

    public string Clave { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaActualizacion { get; set; }
}
