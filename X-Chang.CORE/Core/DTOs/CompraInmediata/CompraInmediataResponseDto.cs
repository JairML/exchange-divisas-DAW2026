using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class CompraInmediataResponseDto
    {
        public int OperacionInmediataId { get; set; }
        public int ParMonedaId { get; set; }

        public string TipoOperacion { get; set; } = "Compra inmediata";
        public string MetodoEjecucion { get; set; } = "Normal";

        public string MonedaOrigen { get; set; } = string.Empty;
        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }
        public decimal CantidadEjecutada { get; set; }

        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public decimal? PrecioPromedio { get; set; }

        public decimal TotalPagado { get; set; }

        public string Estado { get; set; } = string.Empty;
        public DateTime FechaOperacion { get; set; }

        public List<DetalleEjecucionCompraDto> Ejecuciones { get; set; } = new();
    }
}