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

        // Métodos de cobro habilitados para retiro según el país del usuario.
        Task<IEnumerable<MetodosPago>> GetMetodosCobroDisponibles(int usuarioId);

        // Indica si un método de cobro está habilitado para el país del usuario.
        Task<bool> MetodoDisponibleParaUsuario(int metodoPagoId, int usuarioId);

        // Lee un valor de ConfiguracionSistema (ej. MAX_MONTO_OPERACION).
        Task<string?> GetConfiguracion(string clave);

        // Saldo disponible del usuario para una moneda dada.
        Task<decimal> GetSaldoDisponible(int usuarioId, int monedaId);

        // Registra el retiro de forma atómica: crea el retiro, descuenta el saldo completo,
        // registra el movimiento de billetera, el historial y la URL del voucher.
        // Devuelve el id del retiro, el nuevo saldo, la fecha y la URL del voucher.
        Task<(int retiroId, decimal nuevoSaldo, System.DateTime fecha, string voucherUrl)> RegistrarRetiro(
            int usuarioId,
            int monedaId,
            int metodoPagoId,
            decimal montoRetirado,
            decimal comision,
            decimal montoFinalRecibido,
            string correoDestino,
            string codigoIso);
    }
}
