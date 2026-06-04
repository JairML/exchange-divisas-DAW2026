using System;
using System.Collections.Generic;
using System.Text;

namespace X_Chang.CORE.Core.DTOs.CompraInmediata
{
    public class DetalleEjecucionCompraDto
    {
        public int EjecucionId { get; set; }
        public int OfertaVentaId { get; set; }
        public int VendedorId { get; set; }

        public decimal CantidadEjecutada { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalOperacion { get; set; }

        public DateTime FechaEjecucion { get; set; }
    }
}