using System.Threading.Tasks;
using X_Chang.API.Models;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface ICancelacionRepository
    {
        Task<Usuarios?> GetUsuario(int usuarioId);

        // Carga la orden con su par y monedas (origen/destino) relacionados.
        Task<OrdenesCompra?> GetOrden(int ordenCompraId);

        // Carga la oferta con su par y monedas (origen/destino) relacionados.
        Task<OfertasVenta?> GetOferta(int ofertaVentaId);

        // Ejecuta la cancelación de forma atómica. Vuelve a verificar el estado dentro
        // de la transacción (control de concurrencia): si la operación ya no es cancelable
        // devuelve null. En caso de éxito, marca la operación como Cancelada, registra la
        // cancelación, reembolsa el saldo, registra el historial y la notificación.
        Task<(int cancelacionId, decimal nuevoSaldo, System.DateTime fecha)?> EjecutarCancelacion(
            string tipoOperacion,
            int referenciaId,
            int usuarioId,
            int parMonedaId,
            int monedaReembolsoId,
            decimal montoReembolsado,
            decimal cantidadEjecutada,
            decimal cantidadCancelada,
            string correoDestino);
    }
}
