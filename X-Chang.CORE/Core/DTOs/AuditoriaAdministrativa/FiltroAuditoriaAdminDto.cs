using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa
{
    public class FiltroAuditoriaAdminDto
    {
        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public string? Administrador { get; set; }

        public string? UsuarioAfectado { get; set; }

        public string TipoAccion { get; set; } = "Todos";

        public int Pagina { get; set; } = 1;

        public string RegistrosPorPagina { get; set; } = "20";
    }
}