using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class UsuarioAdminResumenDto
    {
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string PaisResidencia { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string TextoBotonAccion { get; set; } = string.Empty;
    }
}