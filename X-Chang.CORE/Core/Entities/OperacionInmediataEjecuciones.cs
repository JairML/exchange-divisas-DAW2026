using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class OperacionInmediataEjecuciones
{
    public int OperacionInmediataEjecucionId { get; set; }

    public int OperacionInmediataId { get; set; }

    public int EjecucionId { get; set; }

    public virtual EjecucionesOrden Ejecucion { get; set; } = null!;

    public virtual OperacionesInmediatas OperacionInmediata { get; set; } = null!;
}
