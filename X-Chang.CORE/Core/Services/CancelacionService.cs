using System.Linq;
using System.Threading.Tasks;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    // US-022: Cancelación de orden u oferta.
    public class CancelacionService : ICancelacionService
    {
        private readonly ICancelacionRepository _cancelacionRepository;

        private const string TipoOrden = "Orden de compra";
        private const string TipoOferta = "Oferta de venta";

        public CancelacionService(ICancelacionRepository cancelacionRepository)
        {
            _cancelacionRepository = cancelacionRepository;
        }

        public async Task<ResultadoOperacion<CancelacionDetalleDTO>> GetDetalle(
            int usuarioId, string tipoOperacion, int referenciaId)
        {
            if (tipoOperacion == TipoOrden)
            {
                var orden = await _cancelacionRepository.GetOrden(referenciaId);
                if (orden == null)
                    return ResultadoOperacion<CancelacionDetalleDTO>.Error("La orden no existe");
                if (orden.UsuarioId != usuarioId)
                    return ResultadoOperacion<CancelacionDetalleDTO>.Error("La orden no pertenece al usuario");

                // Para una orden de compra el saldo se comprometió en la moneda de ORIGEN del par.
                var monedaReembolso = orden.ParMoneda.MonedaOrigen.CodigoIso;
                var par = $"{orden.ParMoneda.MonedaOrigen.CodigoIso}/{orden.ParMoneda.MonedaDestino.CodigoIso}";
                var montoPendiente = orden.TotalComprometido - orden.TotalEjecutado;

                return ResultadoOperacion<CancelacionDetalleDTO>.Ok(new CancelacionDetalleDTO
                {
                    TipoOperacion = TipoOrden,
                    ReferenciaId = orden.OrdenCompraId,
                    Par = par,
                    CantidadOriginal = orden.CantidadOriginal,
                    CantidadEjecutada = orden.CantidadObtenida,
                    CantidadPendiente = orden.CantidadPendiente,
                    PrecioUnitario = orden.PrecioUnitario,
                    MontoPendiente = montoPendiente,
                    MontoReembolso = montoPendiente,
                    MonedaReembolso = monedaReembolso,
                    Estado = orden.Estado,
                    PuedeCancelar = EsCancelable(orden.Estado)
                });
            }

            if (tipoOperacion == TipoOferta)
            {
                var oferta = await _cancelacionRepository.GetOferta(referenciaId);
                if (oferta == null)
                    return ResultadoOperacion<CancelacionDetalleDTO>.Error("La oferta no existe");
                if (oferta.UsuarioId != usuarioId)
                    return ResultadoOperacion<CancelacionDetalleDTO>.Error("La oferta no pertenece al usuario");

                // En una oferta de venta se bloqueó la cantidad a vender en la moneda de ORIGEN;
                // al cancelar se libera la cantidad pendiente.
                var monedaReembolso = oferta.ParMoneda.MonedaOrigen.CodigoIso;
                var par = $"{oferta.ParMoneda.MonedaOrigen.CodigoIso}/{oferta.ParMoneda.MonedaDestino.CodigoIso}";
                var montoPendiente = oferta.TotalEsperado - oferta.TotalRecibido;

                return ResultadoOperacion<CancelacionDetalleDTO>.Ok(new CancelacionDetalleDTO
                {
                    TipoOperacion = TipoOferta,
                    ReferenciaId = oferta.OfertaVentaId,
                    Par = par,
                    CantidadOriginal = oferta.CantidadOriginal,
                    CantidadEjecutada = oferta.CantidadVendida,
                    CantidadPendiente = oferta.CantidadPendiente,
                    PrecioUnitario = oferta.PrecioUnitario,
                    MontoPendiente = montoPendiente,
                    MontoReembolso = oferta.CantidadPendiente,
                    MonedaReembolso = monedaReembolso,
                    Estado = oferta.Estado,
                    PuedeCancelar = EsCancelable(oferta.Estado)
                });
            }

            return ResultadoOperacion<CancelacionDetalleDTO>.Error("Tipo de operación no válido");
        }

        public async Task<ResultadoOperacion<CancelacionResultadoDTO>> Cancelar(
            int usuarioId, CancelacionConfirmarDTO dto)
        {
            var usuario = await _cancelacionRepository.GetUsuario(usuarioId);
            if (usuario == null)
                return ResultadoOperacion<CancelacionResultadoDTO>.Error("Usuario no encontrado");

            string par;
            int parMonedaId;
            int monedaReembolsoId;
            decimal montoReembolsado;
            decimal cantidadEjecutada;
            decimal cantidadCancelada;
            string monedaReembolso;

            if (dto.TipoOperacion == TipoOrden)
            {
                var orden = await _cancelacionRepository.GetOrden(dto.ReferenciaId);
                if (orden == null)
                    return ResultadoOperacion<CancelacionResultadoDTO>.Error("La orden no existe");
                if (orden.UsuarioId != usuarioId)
                    return ResultadoOperacion<CancelacionResultadoDTO>.Error("La orden no pertenece al usuario");
                if (!EsCancelable(orden.Estado))
                    return ResultadoOperacion<CancelacionResultadoDTO>.Error("La operación ya no puede ser cancelada");

                par = $"{orden.ParMoneda.MonedaOrigen.CodigoIso}/{orden.ParMoneda.MonedaDestino.CodigoIso}";
                parMonedaId = orden.ParMonedaId;
                monedaReembolsoId = orden.ParMoneda.MonedaOrigenId;
                monedaReembolso = orden.ParMoneda.MonedaOrigen.CodigoIso;
                montoReembolsado = orden.TotalComprometido - orden.TotalEjecutado;
                cantidadEjecutada = orden.CantidadObtenida;
                cantidadCancelada = orden.CantidadPendiente;
            }
            else if (dto.TipoOperacion == TipoOferta)
            {
                var oferta = await _cancelacionRepository.GetOferta(dto.ReferenciaId);
                if (oferta == null)
                    return ResultadoOperacion<CancelacionResultadoDTO>.Error("La oferta no existe");
                if (oferta.UsuarioId != usuarioId)
                    return ResultadoOperacion<CancelacionResultadoDTO>.Error("La oferta no pertenece al usuario");
                if (!EsCancelable(oferta.Estado))
                    return ResultadoOperacion<CancelacionResultadoDTO>.Error("La operación ya no puede ser cancelada");

                par = $"{oferta.ParMoneda.MonedaOrigen.CodigoIso}/{oferta.ParMoneda.MonedaDestino.CodigoIso}";
                parMonedaId = oferta.ParMonedaId;
                monedaReembolsoId = oferta.ParMoneda.MonedaOrigenId;
                monedaReembolso = oferta.ParMoneda.MonedaOrigen.CodigoIso;
                montoReembolsado = oferta.CantidadPendiente;
                cantidadEjecutada = oferta.CantidadVendida;
                cantidadCancelada = oferta.CantidadPendiente;
            }
            else
            {
                return ResultadoOperacion<CancelacionResultadoDTO>.Error("Tipo de operación no válido");
            }

            // La ejecución vuelve a revalidar el estado dentro de la transacción
            // (control de concurrencia). Si ya no es cancelable, devuelve null.
            var resultado = await _cancelacionRepository.EjecutarCancelacion(
                dto.TipoOperacion,
                dto.ReferenciaId,
                usuarioId,
                parMonedaId,
                monedaReembolsoId,
                montoReembolsado,
                cantidadEjecutada,
                cantidadCancelada,
                usuario.CorreoElectronico);

            if (resultado == null)
                return ResultadoOperacion<CancelacionResultadoDTO>.Error("La operación ya no puede ser cancelada");

            var (cancelacionId, nuevoSaldo, fecha) = resultado.Value;

            return ResultadoOperacion<CancelacionResultadoDTO>.Ok(new CancelacionResultadoDTO
            {
                CancelacionId = cancelacionId,
                TipoOperacion = dto.TipoOperacion,
                Par = par,
                CantidadEjecutada = cantidadEjecutada,
                CantidadCancelada = cantidadCancelada,
                MontoReembolsado = montoReembolsado,
                MonedaReembolso = monedaReembolso,
                NuevoSaldo = nuevoSaldo,
                Estado = "Cancelada",
                FechaCancelacion = fecha
            });
        }

        // Una operación se puede cancelar mientras esté activa o parcialmente ejecutada.
        private static bool EsCancelable(string estado) =>
            estado == "Activa" || estado == "Parcialmente ejecutada";
    }
}
