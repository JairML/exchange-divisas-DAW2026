using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class VentaInmediataRequestDto
    {
        public int ParMonedaId { get; set; }

        public decimal CantidadAVender { get; set; }
    }
}