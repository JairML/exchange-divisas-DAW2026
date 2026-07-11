using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IRetiroRepository
    {
        Task<Usuarios?> GetUsuario(int usuarioId);
        Task<Monedas?> GetMoneda(int monedaId);
        Task<MetodosPago?> GetMetodoPago(int metodoPagoId);
        Task<IEnumerable<MetodosPago>> GetMetodosPagoDisponibles(int usuarioId);
        Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId);
        Task<string?> GetConfiguracion(string clave);
        Task<decimal> GetSaldoDisponible(int usuarioId, int monedaId);

        // Registra el retiro de forma atómica: crea el retiro, descuenta el saldo,
        // registra el movimiento y el historial.
        // Devuelve el id del retiro, el nuevo saldo, la fecha y la URL del voucher.
        Task<(int retiroId, decimal nuevoSaldo, DateTime fecha, string? voucherUrl)> RegistrarRetiro(
            int usuarioId,
            int monedaId,
            int metodoPagoId,
            decimal monto,
            decimal comision,
            decimal montoFinal,
            string correoDestino,
            string codigoIso);
    }
}
