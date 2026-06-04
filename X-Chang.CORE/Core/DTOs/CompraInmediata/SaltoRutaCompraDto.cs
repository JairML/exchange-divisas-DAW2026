using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class SaltoRutaCompraDto
    {
        public int NumeroSalto { get; set; }

        public int ParMonedaId { get; set; }
        public string MonedaOrigen { get; set; } = string.Empty;
        public string MonedaDestino { get; set; } = string.Empty;

        public decimal CantidadConvertida { get; set; }
        public decimal ResultadoObtenido { get; set; }

        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public decimal? PrecioPromedio { get; set; }
    }
}