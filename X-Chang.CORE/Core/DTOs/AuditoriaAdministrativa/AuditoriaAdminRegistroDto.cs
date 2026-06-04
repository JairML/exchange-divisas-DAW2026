using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa
{
    public class AuditoriaAdminRegistroDto
    {
        public int AuditoriaId { get; set; }

        public DateTime FechaHora { get; set; }

        public int AdministradorId { get; set; }

        public string Administrador { get; set; } = string.Empty;

        public int UsuarioAfectadoId { get; set; }

        public string UsuarioAfectado { get; set; } = string.Empty;

        public string TipoAccion { get; set; } = string.Empty;

        public string MensajeRegistrado { get; set; } = string.Empty;
    }
}