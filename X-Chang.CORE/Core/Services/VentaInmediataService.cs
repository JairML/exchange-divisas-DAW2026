using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X_Chang.CORE.Core.DTOs.VentaInmediata;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class VentaInmediataService : IVentaInmediataService
    {
        private readonly IVentaInmediataRepository _ventaRepository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public VentaInmediataService(
            IVentaInmediataRepository ventaRepository,
            ISesionUsuarioRepository sesionRepository)
        {
            _ventaRepository = ventaRepository;
            _sesionRepository = sesionRepository;
        }

        public async Task<ResumenVentaInmediataDto> ObtenerResumenVentaNormalAsync(
            string tokenSesion,
            VentaInmediataRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.CantidadAVender <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            var par = await _ventaRepository.ObtenerParMonedaAsync(request.ParMonedaId);

            if (par == null)
                throw new ArgumentException("El par de monedas no existe.");

            var ordenes = await _ventaRepository.ObtenerOrdenesCompraActivasAsync(request.ParMonedaId);

            var resumen = CalcularResumenVenta(
                request.ParMonedaId,
                ObtenerCodigoMoneda(par.MonedaOrigen),
                ObtenerCodigoMoneda(par.MonedaDestino),
                request.CantidadAVender,
                ordenes);

            // En venta inmediata se vende la moneda destino del par y se recibe la moneda origen.
            // Ejemplo: en PEN/USD, el usuario vende USD y recibe PEN.
            var saldoDisponible = await _ventaRepository.ObtenerSaldoDisponibleAsync(
                usuarioId,
                par.MonedaDestinoId);

            resumen.SaldoSuficiente = saldoDisponible >= request.CantidadAVender;

            if (!resumen.SaldoSuficiente)
                resumen.Mensaje = "Saldo insuficiente.";
            else if (!resumen.LiquidezSuficiente)
                resumen.Mensaje = "Liquidez insuficiente.";
            else
                resumen.Mensaje = "Venta inmediata disponible.";

            return resumen;
        }

        public async Task<VentaInmediataResponseDto> ConfirmarVentaNormalAsync(
            string tokenSesion,
            ConfirmarVentaInmediataRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.CantidadAVender <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            var resumen = await ObtenerResumenVentaNormalAsync(
                tokenSesion,
                new VentaInmediataRequestDto
                {
                    ParMonedaId = request.ParMonedaId,
                    CantidadAVender = request.CantidadAVender
                });

            if (resumen.CantidadEjecutable <= 0)
                throw new InvalidOperationException("No existe liquidez disponible.");

            if (!resumen.LiquidezSuficiente && !request.VenderCantidadDisponible)
                throw new InvalidOperationException("No existe suficiente liquidez para cubrir toda la cantidad solicitada.");

            if (!resumen.SaldoSuficiente)
                throw new InvalidOperationException("Saldo insuficiente.");

            var cantidadAEjecutar = request.VenderCantidadDisponible
                ? resumen.CantidadEjecutable
                : request.CantidadAVender;

            return await _ventaRepository.EjecutarVentaInmediataNormalAsync(
                usuarioId,
                request.ParMonedaId,
                cantidadAEjecutar,
                request.VenderCantidadDisponible);
        }

        public async Task<TiempoEstimadoBusquedaRutaVentaDto> ObtenerTiempoEstimadoBusquedaRutaAsync(
            int cantidadMaximaSaltos)
        {
            ValidarCantidadSaltos(cantidadMaximaSaltos);

            var paresActivos = await _ventaRepository.ObtenerParesActivosAsync();

            var cantidadMonedas = paresActivos
                .Select(p => p.MonedaOrigenId)
                .Concat(paresActivos.Select(p => p.MonedaDestinoId))
                .Distinct()
                .Count();

            var rutasEstimadas = CalcularRutasEstimadas(cantidadMonedas, cantidadMaximaSaltos);
            var tiempoEstimadoSegundos = CalcularTiempoEstimadoSegundos(rutasEstimadas);
            var tiempoEstimadoMs = (int)Math.Ceiling(tiempoEstimadoSegundos * 1000m);
            var tiempoTexto = FormatearTiempo(tiempoEstimadoSegundos);

            return new TiempoEstimadoBusquedaRutaVentaDto
            {
                CantidadMaximaSaltos = cantidadMaximaSaltos,
                CantidadMonedas = cantidadMonedas,
                RutasEstimadas = rutasEstimadas,
                TiempoEstimadoMs = tiempoEstimadoMs,
                TiempoEstimadoSegundos = Math.Round(tiempoEstimadoSegundos, 2),
                TiempoEstimadoTexto = tiempoTexto,
                Mensaje = $"La búsqueda puede tardar aproximadamente {tiempoTexto} porque evaluará {rutasEstimadas:N0} rutas posibles."
            };
        }

        public async Task<ResultadoBusquedaRutaVentaDto> BuscarMejorRutaAsync(
            string tokenSesion,
            BuscarMejorRutaVentaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.CantidadAVender <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            ValidarCantidadSaltos(request.CantidadMaximaSaltos);

            var parObjetivo = await _ventaRepository.ObtenerParMonedaAsync(request.ParMonedaId);

            if (parObjetivo == null)
                throw new ArgumentException("El par de monedas no existe.");

            var resumenNormal = await ObtenerResumenVentaNormalAsync(
                tokenSesion,
                new VentaInmediataRequestDto
                {
                    ParMonedaId = request.ParMonedaId,
                    CantidadAVender = request.CantidadAVender
                });

            var busqueda = await _ventaRepository.CrearBusquedaRutaAsync(
                usuarioId,
                request.ParMonedaId,
                request.CantidadAVender,
                request.CantidadMaximaSaltos,
                (await ObtenerTiempoEstimadoBusquedaRutaAsync(request.CantidadMaximaSaltos)).TiempoEstimadoMs);

            var pares = await _ventaRepository.ObtenerParesActivosAsync();

            // La ruta de una venta inicia en la moneda que el usuario vende
            // y termina en la moneda que recibirá.
            // Ejemplo PEN/USD: USD -> ... -> PEN.
            var rutas = GenerarRutas(
                parObjetivo.MonedaDestinoId,
                parObjetivo.MonedaOrigenId,
                request.CantidadMaximaSaltos,
                pares);

            ResultadoBusquedaRutaVentaDto? mejorRuta = null;

            foreach (var ruta in rutas)
            {
                var resultadoRuta = await EvaluarRutaAsync(
                    request.CantidadAVender,
                    ruta,
                    resumenNormal.TotalEstimadoARecibir);

                if (resultadoRuta == null)
                    continue;

                if (mejorRuta == null || resultadoRuta.TotalRutaEncontrada > mejorRuta.TotalRutaEncontrada)
                    mejorRuta = resultadoRuta;
            }

            if (mejorRuta == null || mejorRuta.TotalRutaEncontrada <= resumenNormal.TotalEstimadoARecibir)
            {
                await _ventaRepository.FinalizarBusquedaRutaSinResultadoAsync(busqueda.BusquedaRutaId);

                return new ResultadoBusquedaRutaVentaDto
                {
                    BusquedaRutaId = busqueda.BusquedaRutaId,
                    ParMonedaId = request.ParMonedaId,
                    MonedaOrigen = ObtenerCodigoMoneda(parObjetivo.MonedaOrigen),
                    MonedaDestino = ObtenerCodigoMoneda(parObjetivo.MonedaDestino),
                    CantidadSolicitada = request.CantidadAVender,
                    CantidadSaltos = request.CantidadMaximaSaltos,
                    TotalVentaNormal = resumenNormal.TotalEstimadoARecibir,
                    TotalRutaEncontrada = 0,
                    GananciaEstimada = 0,
                    RutaEncontrada = false,
                    Mensaje = "No se encontró una ruta más rentable"
                };
            }

            mejorRuta.BusquedaRutaId = busqueda.BusquedaRutaId;
            mejorRuta.ParMonedaId = request.ParMonedaId;
            mejorRuta.MonedaOrigen = ObtenerCodigoMoneda(parObjetivo.MonedaOrigen);
            mejorRuta.MonedaDestino = ObtenerCodigoMoneda(parObjetivo.MonedaDestino);
            mejorRuta.CantidadSolicitada = request.CantidadAVender;
            mejorRuta.TotalVentaNormal = resumenNormal.TotalEstimadoARecibir;
            mejorRuta.GananciaEstimada = mejorRuta.TotalRutaEncontrada - resumenNormal.TotalEstimadoARecibir;
            mejorRuta.RutaEncontrada = true;
            mejorRuta.Mensaje = "Ruta más rentable encontrada.";

            await _ventaRepository.GuardarResultadoRutaAsync(
                busqueda.BusquedaRutaId,
                mejorRuta);

            return mejorRuta;
        }

        public async Task<bool> CancelarBusquedaRutaAsync(
            string tokenSesion,
            int busquedaRutaId)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            var busqueda = await _ventaRepository.ObtenerBusquedaRutaAsync(
                busquedaRutaId,
                usuarioId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            if (busqueda.Estado == "Completada")
                throw new InvalidOperationException("La búsqueda ya fue completada.");

            await _ventaRepository.CancelarBusquedaRutaAsync(busquedaRutaId);

            return true;
        }

        public async Task<VentaInmediataResponseDto> ConfirmarVentaPorRutaAsync(
            string tokenSesion,
            ConfirmarVentaRutaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            var busqueda = await _ventaRepository.ObtenerBusquedaRutaConSaltosAsync(
                request.BusquedaRutaId,
                usuarioId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            if (busqueda.Estado != "Completada")
                throw new InvalidOperationException("La búsqueda no tiene una ruta disponible para confirmar.");

            return await _ventaRepository.EjecutarVentaInmediataPorRutaAsync(
                usuarioId,
                request.BusquedaRutaId);
        }

        private async Task<int> ObtenerUsuarioIdDesdeSesionAsync(string tokenSesion)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);

            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            if (sesion.Usuario.Estado == "Restringido")
                throw new InvalidOperationException("Su cuenta se encuentra restringida y no puede realizar ventas inmediatas.");

            return sesion.UsuarioId;
        }

        private static void ValidarCantidadSaltos(int cantidadMaximaSaltos)
        {
            if (cantidadMaximaSaltos < 1)
                throw new ArgumentException("Mínimo 1 salto.");

            if (cantidadMaximaSaltos > 5)
                throw new ArgumentException("Máximo 5 saltos.");
        }

        private static ResumenVentaInmediataDto CalcularResumenVenta(
            int parMonedaId,
            string monedaOrigen,
            string monedaDestino,
            decimal cantidadSolicitada,
            List<OrdenesCompra> ordenes)
        {
            decimal cantidadPendiente = cantidadSolicitada;
            decimal cantidadEjecutable = 0;
            decimal totalEstimadoARecibir = 0;

            var preciosUsados = new List<decimal>();

            foreach (var orden in ordenes.OrderByDescending(o => o.PrecioUnitario))
            {
                if (cantidadPendiente <= 0)
                    break;

                var cantidadTomada = Math.Min(cantidadPendiente, orden.CantidadPendiente);
                var subtotal = cantidadTomada * orden.PrecioUnitario;

                cantidadEjecutable += cantidadTomada;
                totalEstimadoARecibir += subtotal;
                cantidadPendiente -= cantidadTomada;
                preciosUsados.Add(orden.PrecioUnitario);
            }

            return new ResumenVentaInmediataDto
            {
                ParMonedaId = parMonedaId,
                MonedaOrigen = monedaOrigen,
                MonedaDestino = monedaDestino,
                CantidadSolicitada = cantidadSolicitada,
                CantidadDisponible = ordenes.Sum(o => o.CantidadPendiente),
                CantidadEjecutable = cantidadEjecutable,
                PrecioMinimoCompra = preciosUsados.Count > 0 ? preciosUsados.Min() : null,
                PrecioMaximoCompra = preciosUsados.Count > 0 ? preciosUsados.Max() : null,
                PrecioPromedioCompra = cantidadEjecutable > 0 ? totalEstimadoARecibir / cantidadEjecutable : null,
                TotalEstimadoARecibir = totalEstimadoARecibir,
                LiquidezSuficiente = cantidadEjecutable >= cantidadSolicitada,
                SaldoSuficiente = false
            };
        }

        private static List<List<ParesMoneda>> GenerarRutas(
            int monedaOrigenId,
            int monedaDestinoId,
            int maxSaltos,
            List<ParesMoneda> pares)
        {
            var rutas = new List<List<ParesMoneda>>();

            void Buscar(
                int monedaActualId,
                List<ParesMoneda> caminoActual,
                HashSet<int> monedasVisitadas)
            {
                if (caminoActual.Count > maxSaltos)
                    return;

                if (monedaActualId == monedaDestinoId && caminoActual.Count > 0)
                {
                    rutas.Add(new List<ParesMoneda>(caminoActual));
                    return;
                }

                foreach (var par in pares.Where(p => p.MonedaOrigenId == monedaActualId))
                {
                    if (monedasVisitadas.Contains(par.MonedaDestinoId))
                        continue;

                    caminoActual.Add(par);
                    monedasVisitadas.Add(par.MonedaDestinoId);

                    Buscar(par.MonedaDestinoId, caminoActual, monedasVisitadas);

                    caminoActual.RemoveAt(caminoActual.Count - 1);
                    monedasVisitadas.Remove(par.MonedaDestinoId);
                }
            }

            Buscar(monedaOrigenId, new List<ParesMoneda>(), new HashSet<int> { monedaOrigenId });

            return rutas;
        }

        private async Task<ResultadoBusquedaRutaVentaDto?> EvaluarRutaAsync(
            decimal cantidadInicialAVender,
            List<ParesMoneda> ruta,
            decimal totalVentaNormal)
        {
            decimal cantidadActualAVender = cantidadInicialAVender;
            var saltos = new List<SaltoRutaVentaDto>();

            for (int i = 0; i < ruta.Count; i++)
            {
                var par = ruta[i];

                var ordenes = await _ventaRepository.ObtenerOrdenesCompraActivasAsync(
                    par.ParMonedaId);

                var resumen = CalcularResumenVenta(
                    par.ParMonedaId,
                    ObtenerCodigoMoneda(par.MonedaOrigen),
                    ObtenerCodigoMoneda(par.MonedaDestino),
                    cantidadActualAVender,
                    ordenes);

                if (!resumen.LiquidezSuficiente)
                    return null;

                saltos.Add(new SaltoRutaVentaDto
                {
                    NumeroSalto = i + 1,
                    ParMonedaId = par.ParMonedaId,
                    MonedaOrigen = ObtenerCodigoMoneda(par.MonedaOrigen),
                    MonedaDestino = ObtenerCodigoMoneda(par.MonedaDestino),
                    CantidadVendida = cantidadActualAVender,
                    ResultadoObtenido = resumen.TotalEstimadoARecibir,
                    PrecioMinimo = resumen.PrecioMinimoCompra,
                    PrecioMaximo = resumen.PrecioMaximoCompra,
                    PrecioPromedio = resumen.PrecioPromedioCompra
                });

                cantidadActualAVender = resumen.TotalEstimadoARecibir;
            }

            return new ResultadoBusquedaRutaVentaDto
            {
                CantidadSaltos = saltos.Count,
                TotalVentaNormal = totalVentaNormal,
                TotalRutaEncontrada = cantidadActualAVender,
                GananciaEstimada = cantidadActualAVender - totalVentaNormal,
                PrecioMinimo = saltos
                    .Where(s => s.PrecioMinimo.HasValue)
                    .Select(s => s.PrecioMinimo!.Value)
                    .DefaultIfEmpty()
                    .Min(),
                PrecioMaximo = saltos
                    .Where(s => s.PrecioMaximo.HasValue)
                    .Select(s => s.PrecioMaximo!.Value)
                    .DefaultIfEmpty()
                    .Max(),
                PrecioPromedio = cantidadInicialAVender > 0
                    ? cantidadActualAVender / cantidadInicialAVender
                    : null,
                RutaEncontrada = cantidadActualAVender > totalVentaNormal,
                Saltos = saltos
            };
        }

        private static string ObtenerCodigoMoneda(Monedas? moneda)
        {
            if (moneda == null)
                return string.Empty;

            return moneda.CodigoIso;
        }
        private static long CalcularRutasEstimadas(int cantidadMonedas, int saltos)
        {
            if (cantidadMonedas < 2)
                return 0;

            if (saltos < 1)
                return 0;

            var monedasIntermedias = cantidadMonedas - 2;
            long total = 0;

            for (var k = 1; k <= saltos; k++)
            {
                var intermediasNecesarias = k - 1;

                if (intermediasNecesarias > monedasIntermedias)
                    break;

                long permutaciones = 1;

                for (var i = 0; i < intermediasNecesarias; i++)
                {
                    permutaciones *= monedasIntermedias - i;
                }

                total += permutaciones;
            }

            return total;
        }

        private static decimal CalcularTiempoEstimadoSegundos(long rutasEstimadas)
        {
            const decimal segundosPorRuta = 0.001m;

            if (rutasEstimadas <= 0)
                return 0;

            var estimado = rutasEstimadas * segundosPorRuta;

            return Math.Max(0.1m, estimado);
        }

        private static string FormatearTiempo(decimal segundos)
        {
            if (segundos < 1)
                return "menos de 1 segundo";

            if (segundos < 60)
                return $"{Math.Ceiling(segundos)} segundos";

            var minutos = Math.Floor(segundos / 60);
            var segundosRestantes = Math.Ceiling(segundos % 60);

            return $"{minutos} min {segundosRestantes} s";
        }
    }
}