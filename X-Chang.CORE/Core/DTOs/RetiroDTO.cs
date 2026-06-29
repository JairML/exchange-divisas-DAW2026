using System;

namespace X_Chang.CORE.Core.DTOs
{
    // Método de cobro disponible para el usuario (análogo a MetodoPagoDTO en depósito).
    public class MetodoCobroDTO
    {
        public int MetodoPagoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal ComisionPorcentaje { get; set; }
        public decimal ComisionFija { get; set; }
    }

    // Entrada para calcular el resumen antes de confirmar el retiro.
    public class RetiroCalcularDTO
    {
        public int MonedaId { get; set; }
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    // Resumen mostrado al usuario antes de confirmar el retiro.
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

    // Entrada para confirmar y registrar el retiro.
    public class RetiroCreateDTO
    {
        public int MonedaId { get; set; }
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    // Resultado del retiro ya registrado.
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
