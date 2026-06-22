using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class ResultadoBusquedaRutaVentaDto
    {
        public int BusquedaRutaId { get; set; }

        public int ParMonedaId { get; set; }

        public string MonedaOrigen { get; set; } = string.Empty;

        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }

        public int CantidadSaltos { get; set; }

        public decimal TotalVentaNormal { get; set; }

        public decimal TotalRutaEncontrada { get; set; }

        public decimal GananciaEstimada { get; set; }

        public decimal? PrecioMinimo { get; set; }

        public decimal? PrecioMaximo { get; set; }

        public decimal? PrecioPromedio { get; set; }

        public bool RutaEncontrada { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public List<SaltoRutaVentaDto> Saltos { get; set; } = new();
    }
}