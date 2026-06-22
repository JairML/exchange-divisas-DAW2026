using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class ResultadoBusquedaRutaCompraDto
    {
        public int BusquedaRutaId { get; set; }

        public int ParMonedaId { get; set; }

        public string MonedaOrigen { get; set; } = string.Empty;

        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadSolicitada { get; set; }

        public int CantidadSaltos { get; set; }

        public decimal TotalCompraNormal { get; set; }

        public decimal TotalRutaEncontrada { get; set; }

        public decimal AhorroEstimado { get; set; }

        public decimal? PrecioMinimo { get; set; }

        public decimal? PrecioMaximo { get; set; }

        public decimal? PrecioPromedio { get; set; }

        public bool RutaEncontrada { get; set; }

        public string? Mensaje { get; set; }

        public List<SaltoRutaCompraDto> Saltos { get; set; } = new();
    }
}