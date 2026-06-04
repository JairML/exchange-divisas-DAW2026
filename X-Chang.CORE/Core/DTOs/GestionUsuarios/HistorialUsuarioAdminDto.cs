using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.GestionUsuarios
{
    public class HistorialUsuarioAdminDto
    {
        public int HistorialId { get; set; }

        public string TipoOperacion { get; set; } = string.Empty;

        public int ReferenciaId { get; set; }

        public int? ParMonedaId { get; set; }

        public string? ParMoneda { get; set; }

        public int? MonedaId { get; set; }

        public string? Moneda { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string? MetodoEjecucion { get; set; }

        public DateTime FechaHora { get; set; }
    }
}