using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    // US-008: Retiro de dinero de la billetera.
    public class RetiroService : IRetiroService
    {
        private readonly IRetiroRepository _retiroRepository;

        // Monto máximo por defecto si no estuviera definido en ConfiguracionSistema.
        private const decimal MaxMontoPorDefecto = 1000000m;

        public RetiroService(IRetiroRepository retiroRepository)
        {
            _retiroRepository = retiroRepository;
        }

        public async Task<IEnumerable<MetodoPagoDTO>> GetMetodosCobro(int usuarioId)
        {
            var metodos = await _retiroRepository.GetMetodosCobroDisponibles(usuarioId);

            return metodos.Select(m => new MetodoPagoDTO
            {
                MetodoPagoId = m.MetodoPagoId,
                Nombre = m.Nombre,
                ComisionPorcentaje = m.ComisionPorcentaje,
                ComisionFija = m.ComisionFija
            }).ToList();
        }

        public async Task<ResultadoOperacion<RetiroResumenDTO>> Calcular(int usuarioId, RetiroCalcularDTO dto)
        {
            var (error, moneda, metodo, _) = await Validar(usuarioId, dto.MonedaId, dto.MetodoPagoId, dto.Monto);
            if (error != null)
                return ResultadoOperacion<RetiroResumenDTO>.Error(error);

            var comision = CalcularComision(dto.Monto, metodo!.ComisionPorcentaje, metodo.ComisionFija);
            var montoFinal = dto.Monto - comision;

            return ResultadoOperacion<RetiroResumenDTO>.Ok(new RetiroResumenDTO
            {
                MonedaId = moneda!.MonedaId,
                CodigoISO = moneda.CodigoIso,
                MontoARetirar = dto.Monto,
                MetodoPagoId = metodo.MetodoPagoId,
                MetodoCobro = metodo.Nombre,
                ComisionAplicada = comision,
                MontoFinalRecibido = montoFinal
            });
        }

        public async Task<ResultadoOperacion<RetiroResultadoDTO>> RegistrarRetiro(int usuarioId, RetiroCreateDTO dto)
        {
            var (error, moneda, metodo, usuario) = await Validar(usuarioId, dto.MonedaId, dto.MetodoPagoId, dto.Monto);
            if (error != null)
                return ResultadoOperacion<RetiroResultadoDTO>.Error(error);

            var comision = CalcularComision(dto.Monto, metodo!.ComisionPorcentaje, metodo.ComisionFija);
            var montoFinal = dto.Monto - comision;

            var (retiroId, nuevoSaldo, fecha, voucherUrl) = await _retiroRepository.RegistrarRetiro(
                usuarioId,
                moneda!.MonedaId,
                metodo.MetodoPagoId,
                dto.Monto,
                comision,
                montoFinal,
                usuario!.CorreoElectronico,
                moneda.CodigoIso);

            return ResultadoOperacion<RetiroResultadoDTO>.Ok(new RetiroResultadoDTO
            {
                RetiroId = retiroId,
                MonedaId = moneda.MonedaId,
                CodigoISO = moneda.CodigoIso,
                MontoRetirado = dto.Monto,
                ComisionAplicada = comision,
                MontoFinalRecibido = montoFinal,
                Estado = "Completada",
                VoucherUrl = voucherUrl,
                NuevoSaldo = nuevoSaldo,
                FechaRetiro = fecha
            });
        }

        // Validaciones comunes a "calcular" y "registrar", en el orden de los criterios
        // de aceptación. Devuelve el primer mensaje de error encontrado (o null si todo OK)
        // junto con las entidades ya cargadas para reutilizarlas.
        private async Task<(string? error, Monedas? moneda, MetodosPago? metodo, Usuarios? usuario)> Validar(
            int usuarioId, int monedaId, int metodoPagoId, decimal monto)
        {
            var usuario = await _retiroRepository.GetUsuario(usuarioId);
            if (usuario == null)
                return ("Usuario no encontrado", null, null, null);

            // US-020: un usuario restringido no puede operar.
            if (usuario.Estado == "Restringido")
                return ("Su cuenta se encuentra restringida y no puede realizar retiros", null, null, null);

            if (monedaId <= 0)
                return ("Seleccione una moneda", null, null, null);

            var moneda = await _retiroRepository.GetMoneda(monedaId);
            if (moneda == null || !moneda.Activa)
                return ("Seleccione una moneda", null, null, null);

            if (metodoPagoId <= 0)
                return ("Seleccione un método de cobro", moneda, null, usuario);

            var metodo = await _retiroRepository.GetMetodoPago(metodoPagoId);
            if (metodo == null)
                return ("Seleccione un método de cobro", moneda, null, usuario);

            // El método debe estar habilitado para el país del usuario.
            var disponible = await _retiroRepository.MetodoDisponibleParaUsuario(metodoPagoId, usuarioId);
            if (!disponible)
                return ("El método de cobro no está disponible en su país", moneda, null, usuario);

            if (monto <= 0m)
                return ("El monto debe ser mayor a 0", moneda, metodo, usuario);

            var maximo = await GetMontoMaximo();
            if (monto > maximo)
                return ("Monto máximo excedido", moneda, metodo, usuario);

            var saldo = await _retiroRepository.GetSaldoDisponible(usuarioId, monedaId);
            if (saldo < monto)
                return ("Saldo insuficiente para realizar el retiro", moneda, metodo, usuario);

            return (null, moneda, metodo, usuario);
        }

        // comisión = monto * (porcentaje / 100) + comisión fija. Redondeada a 8 decimales
        // para respetar la precisión DECIMAL(28,8) de la base de datos.
        private static decimal CalcularComision(decimal monto, decimal comisionPorcentaje, decimal comisionFija)
        {
            var comision = monto * (comisionPorcentaje / 100m) + comisionFija;
            return Math.Round(comision, 8, MidpointRounding.AwayFromZero);
        }

        private async Task<decimal> GetMontoMaximo()
        {
            var valor = await _retiroRepository.GetConfiguracion("MAX_MONTO_OPERACION");
            if (!string.IsNullOrWhiteSpace(valor)
                && decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var max))
            {
                return max;
            }
            return MaxMontoPorDefecto;
        }
    }
}
