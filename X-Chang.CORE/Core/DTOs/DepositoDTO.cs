using System;

namespace X_Chang.CORE.Core.DTOs
{
    // Método de pago disponible para el usuario según su país de residencia.
    public class MetodoPagoDTO
    {
        public int MetodoPagoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal ComisionPorcentaje { get; set; }
        public decimal ComisionFija { get; set; }
    }

    // Entrada para calcular el resumen (comisión y total) antes de confirmar el depósito.
    public class DepositoCalcularDTO
    {
        public int MonedaId { get; set; }
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    // Resumen mostrado al usuario antes de confirmar el depósito.
    public class DepositoResumenDTO
    {
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public decimal MontoDepositado { get; set; }
        public int MetodoPagoId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal ComisionAplicada { get; set; }
        public decimal TotalPagado { get; set; }
    }

    // Entrada para confirmar y registrar el depósito.
    public class DepositoCreateDTO
    {
        public int MonedaId { get; set; }
        public int MetodoPagoId { get; set; }
        public decimal Monto { get; set; }
    }

    // Resultado del depósito ya registrado.
    public class DepositoResultadoDTO
    {
        public int DepositoId { get; set; }
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public decimal MontoDepositado { get; set; }
        public decimal ComisionAplicada { get; set; }
        public decimal TotalPagado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? VoucherUrl { get; set; }
        public decimal NuevoSaldo { get; set; }
        public DateTime FechaDeposito { get; set; }
    }
}
