using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class ConfirmarVentaInmediataRequestDto
    {
        public int ParMonedaId { get; set; }

        public decimal CantidadAVender { get; set; }

        public bool VenderCantidadDisponible { get; set; }
    }
}