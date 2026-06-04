using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.VentaInmediata
{
    public class DetalleEjecucionVentaDto
    {
        public int EjecucionId { get; set; }

        public int OrdenCompraId { get; set; }

        public int CompradorId { get; set; }

        public decimal CantidadEjecutada { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal TotalOperacion { get; set; }

        public DateTime FechaEjecucion { get; set; }
    }
}