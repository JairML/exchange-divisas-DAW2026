using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class ResumenVentaInmediataDto
    {
        public int ParMonedaId { get; set; }

        public string MonedaOrigen { get; set; } = string.Empty;

        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }

        public decimal CantidadDisponible { get; set; }

        public decimal CantidadEjecutable { get; set; }

        public decimal? PrecioMinimoCompra { get; set; }

        public decimal? PrecioMaximoCompra { get; set; }

        public decimal? PrecioPromedioCompra { get; set; }

        public decimal TotalEstimadoARecibir { get; set; }

        public bool LiquidezSuficiente { get; set; }

        public bool SaldoSuficiente { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}