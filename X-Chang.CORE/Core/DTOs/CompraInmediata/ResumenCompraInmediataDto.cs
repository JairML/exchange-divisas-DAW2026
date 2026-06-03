using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class ResumenCompraInmediataDto
    {
        public int ParMonedaId { get; set; }
        public string MonedaOrigen { get; set; } = string.Empty;
        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal CantidadEjecutable { get; set; }

        public decimal? PrecioMinimoVenta { get; set; }
        public decimal? PrecioMaximoVenta { get; set; }
        public decimal? PrecioPromedioVenta { get; set; }

        public decimal TotalEstimado { get; set; }
        public bool LiquidezSuficiente { get; set; }
        public bool SaldoSuficiente { get; set; }

        public string? Mensaje { get; set; }
    }
}