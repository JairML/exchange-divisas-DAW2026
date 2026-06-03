using System;
using System.Collections.Generic;

namespace X_Chang.API.Models;

public partial class Roles
{
    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();
}
