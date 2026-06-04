using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class FiltroUsuariosAdminDto
    {
        public string? NombreUsuario { get; set; }

        public string? CorreoElectronico { get; set; }

        public string Estado { get; set; } = "Todos";
    }
}