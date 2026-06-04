using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class TiempoEstimadoBusquedaRutaDto
    {
        public int CantidadMaximaSaltos { get; set; }
        public int TiempoEstimadoMs { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}