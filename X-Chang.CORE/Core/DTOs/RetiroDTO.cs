using System;

namespace X_Chang.CORE.Core.DTOs
{
    public class RetiroCalcularDTO
    {
        public int MonedaId { get; set; }
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    public class RetiroResumenDTO
    {
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public decimal MontoRetirado { get; set; }
        public int MetodoPagoId { get; set; }
        public string MetodoCobro { get; set; } = string.Empty;
        public decimal ComisionAplicada { get; set; }
        public decimal MontoFinalRecibido { get; set; }
        public decimal SaldoDisponible { get; set; }
    }

    public class RetiroCreateDTO
    {
        public int MonedaId { get; set; }
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    public class RetiroResultadoDTO
    {
        public int RetiroId { get; set; }
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public decimal MontoRetirado { get; set; }
        public decimal ComisionAplicada { get; set; }
        public decimal MontoFinalRecibido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? VoucherUrl { get; set; }
        public decimal NuevoSaldo { get; set; }
        public DateTime FechaRetiro { get; set; }
    }
}
