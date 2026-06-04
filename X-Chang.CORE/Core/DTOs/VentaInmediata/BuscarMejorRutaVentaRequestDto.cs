using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class BuscarMejorRutaVentaRequestDto
    {
        public int ParMonedaId { get; set; }

        public decimal CantidadAVender { get; set; }

        public int CantidadMaximaSaltos { get; set; } = 1;
    }
}