using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa
{
    public class AuditoriaAdminPaginadoDto
    {
        public List<AuditoriaAdminRegistroDto> Registros { get; set; } = new();

        public int PaginaActual { get; set; }

        public int TotalPaginas { get; set; }

        public int TotalRegistros { get; set; }

        public string RegistrosPorPagina { get; set; } = "20";

        public bool TienePaginaAnterior { get; set; }

        public bool TienePaginaSiguiente { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}