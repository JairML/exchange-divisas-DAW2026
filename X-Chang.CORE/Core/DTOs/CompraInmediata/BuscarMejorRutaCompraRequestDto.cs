using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class BuscarMejorRutaCompraRequestDto
    {
        public int ParMonedaId { get; set; }
        public decimal CantidadAObtener { get; set; }
        public int CantidadMaximaSaltos { get; set; } = 1;
    }
}