using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using X_Chang.API.Models;
using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    // US-007: Depósito de dinero a la billetera.
    public class DepositoService : IDepositoService
    {
        private readonly IDepositoRepository _depositoRepository;

        // Monto máximo por defecto si no estuviera definido en ConfiguracionSistema.
        private const decimal MaxMontoPorDefecto = 1000000m;

        public DepositoService(IDepositoRepository depositoRepository)
        {
            _depositoRepository = depositoRepository;
        }

        public async Task<IEnumerable<MetodoPagoDTO>> GetMetodosPago(int usuarioId)
        {
            var metodos = await _depositoRepository.GetMetodosPagoDisponibles(usuarioId);

            return metodos.Select(m => new MetodoPagoDTO
            {
                MetodoPagoId = m.MetodoPagoId,
                Nombre = m.Nombre,
                ComisionPorcentaje = m.ComisionPorcentaje,
                ComisionFija = m.ComisionFija
            }).ToList();
        }

        public async Task<ResultadoOperacion<DepositoResumenDTO>> Calcular(int usuarioId, DepositoCalcularDTO dto)
        {
            var (error, moneda, metodo, _) = await Validar(usuarioId, dto.MonedaId, dto.MetodoPagoId, dto.Monto);
            if (error != null)
                return ResultadoOperacion<DepositoResumenDTO>.Error(error);

            var comision = CalcularComision(dto.Monto, metodo!.ComisionPorcentaje, metodo.ComisionFija);
            var total = dto.Monto + comision;

            return ResultadoOperacion<DepositoResumenDTO>.Ok(new DepositoResumenDTO
            {
                MonedaId = moneda!.MonedaId,
                CodigoISO = moneda.CodigoIso,
                MontoDepositado = dto.Monto,
                MetodoPagoId = metodo.MetodoPagoId,
                MetodoPago = metodo.Nombre,
                ComisionAplicada = comision,
                TotalPagado = total
            });
        }

        public async Task<ResultadoOperacion<DepositoResultadoDTO>> RegistrarDeposito(int usuarioId, DepositoCreateDTO dto)
        {
            var (error, moneda, metodo, usuario) = await Validar(usuarioId, dto.MonedaId, dto.MetodoPagoId, dto.Monto);
            if (error != null)
                return ResultadoOperacion<DepositoResultadoDTO>.Error(error);

            var comision = CalcularComision(dto.Monto, metodo!.ComisionPorcentaje, metodo.ComisionFija);
            var total = dto.Monto + comision;

            var (depositoId, nuevoSaldo, fecha, voucherUrl) = await _depositoRepository.RegistrarDeposito(
                usuarioId,
                moneda!.MonedaId,
                metodo.MetodoPagoId,
                dto.Monto,
                comision,
                total,
                usuario!.CorreoElectronico,
                moneda.CodigoIso);

            return ResultadoOperacion<DepositoResultadoDTO>.Ok(new DepositoResultadoDTO
            {
                DepositoId = depositoId,
                MonedaId = moneda.MonedaId,
                CodigoISO = moneda.CodigoIso,
                MontoDepositado = dto.Monto,
                ComisionAplicada = comision,
                TotalPagado = total,
                Estado = "Completada",
                VoucherUrl = voucherUrl,
                NuevoSaldo = nuevoSaldo,
                FechaDeposito = fecha
            });
        }

        // Validaciones comunes a "calcular" y "registrar", en el orden de los criterios
        // de aceptación. Devuelve el primer mensaje de error encontrado (o null si todo OK)
        // junto con las entidades ya cargadas para reutilizarlas.
        private async Task<(string? error, Monedas? moneda, MetodosPago? metodo, Usuarios? usuario)> Validar(
            int usuarioId, int monedaId, int metodoPagoId, decimal monto)
        {
            var usuario = await _depositoRepository.GetUsuario(usuarioId);
            if (usuario == null)
                return ("Usuario no encontrado", null, null, null);

            // US-020: un usuario restringido no puede operar.
            if (usuario.Estado == "Restringido")
                return ("Su cuenta se encuentra restringida y no puede realizar depósitos", null, null, null);

            if (monedaId <= 0)
                return ("Seleccione una moneda", null, null, null);

            var moneda = await _depositoRepository.GetMoneda(monedaId);
            if (moneda == null || !moneda.Activa)
                return ("Seleccione una moneda", null, null, null);

            if (metodoPagoId <= 0)
                return ("Seleccione un método de pago", moneda, null, usuario);

            var metodo = await _depositoRepository.GetMetodoPago(metodoPagoId);
            if (metodo == null)
                return ("Seleccione un método de pago", moneda, null, usuario);

            // El método debe estar habilitado para el país del usuario.
            var disponible = await _depositoRepository.MetodoDisponibleParaUsuario(metodoPagoId, usuarioId);
            if (!disponible)
                return ("El método de pago no está disponible en su país", moneda, null, usuario);

            if (monto <= 0m)
                return ("El monto debe ser mayor a 0", moneda, metodo, usuario);

            var maximo = await GetMontoMaximo();
            if (monto > maximo)
                return ("Monto máximo excedido", moneda, metodo, usuario);

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
            var valor = await _depositoRepository.GetConfiguracion("MAX_MONTO_OPERACION");
            if (!string.IsNullOrWhiteSpace(valor)
                && decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var max))
            {
                return max;
            }
            return MaxMontoPorDefecto;
        }
    }
}
