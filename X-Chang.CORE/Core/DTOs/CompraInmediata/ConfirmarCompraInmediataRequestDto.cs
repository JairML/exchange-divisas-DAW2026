using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class ConfirmarCompraInmediataRequestDto
    {
        public int ParMonedaId { get; set; }
        public decimal CantidadAObtener { get; set; }

        public bool ComprarCantidadDisponible { get; set; } = false;
    }
}