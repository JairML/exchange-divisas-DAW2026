using System.Collections.Generic;
using System.Threading.Tasks;
using X_Chang.API.Models;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IDepositoRepository
    {
        Task<Usuarios?> GetUsuario(int usuarioId);
        Task<Monedas?> GetMoneda(int monedaId);
        Task<MetodosPago?> GetMetodoPago(int metodoPagoId);

        // Métodos de pago habilitados para depósito según el país del usuario.
        Task<IEnumerable<MetodosPago>> GetMetodosPagoDisponibles(int usuarioId);

        // Indica si un método de pago está habilitado para depósito en el país del usuario.
        Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId);

        // Lee un valor de ConfiguracionSistema (ej. MAX_MONTO_OPERACION).
        Task<string?> GetConfiguracion(string clave);

        // Registra el depósito de forma atómica: crea el depósito, actualiza el saldo,
        // registra el movimiento de billetera, el historial y la notificación con voucher.
        // Devuelve el id del depósito, el nuevo saldo, la fecha y la URL del voucher.
        Task<(int depositoId, decimal nuevoSaldo, System.DateTime fecha, string voucherUrl)> RegistrarDeposito(
            int usuarioId,
            int monedaId,
            int metodoPagoId,
            decimal monto,
            decimal comision,
            decimal total,
            string correoDestino,
            string codigoIso);
    }
}
