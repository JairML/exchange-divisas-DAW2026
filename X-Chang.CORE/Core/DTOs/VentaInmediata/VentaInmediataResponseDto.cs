using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class VentaInmediataResponseDto
    {
        public int OperacionInmediataId { get; set; }

        public int ParMonedaId { get; set; }

        public string TipoOperacion { get; set; } = string.Empty;

        public string MetodoEjecucion { get; set; } = string.Empty;

        public string MonedaOrigen { get; set; } = string.Empty;

        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }

        public decimal CantidadEjecutada { get; set; }

        public decimal? PrecioMinimo { get; set; }

        public decimal? PrecioMaximo { get; set; }

        public decimal? PrecioPromedio { get; set; }

        public decimal TotalRecibido { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaOperacion { get; set; }

        public List<DetalleEjecucionVentaDto> Ejecuciones { get; set; } = new();
    }
}