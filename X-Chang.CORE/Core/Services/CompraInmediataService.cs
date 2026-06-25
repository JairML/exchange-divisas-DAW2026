using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.CompraInmediata;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class CompraInmediataService : ICompraInmediataService
    {
        private readonly ICompraInmediataRepository _compraRepository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public CompraInmediataService(
            ICompraInmediataRepository compraRepository,
            ISesionUsuarioRepository sesionRepository)
        {
            _compraRepository = compraRepository;
            _sesionRepository = sesionRepository;
        }

        public async Task<ResumenCompraInmediataDto> ObtenerResumenCompraNormalAsync(
            string tokenSesion,
            CompraInmediataRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.CantidadAObtener <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            var par = await _compraRepository.ObtenerParMonedaAsync(request.ParMonedaId);

            if (par == null)
                throw new ArgumentException("El par de monedas no existe.");

            var ofertas = await _compraRepository.ObtenerOfertasVentaActivasAsync(request.ParMonedaId);

            var resumen = CalcularResumenCompra(
                request.ParMonedaId,
                ObtenerCodigoMoneda(par.MonedaOrigen),
                ObtenerCodigoMoneda(par.MonedaDestino),
                request.CantidadAObtener,
                ofertas);

            var saldoDisponible = await _compraRepository.ObtenerSaldoDisponibleAsync(
                usuarioId,
                par.MonedaOrigenId);

            resumen.SaldoSuficiente = saldoDisponible >= resumen.TotalEstimado;

            if (!resumen.SaldoSuficiente)
                resumen.Mensaje = "Saldo insuficiente.";
            else if (!resumen.LiquidezSuficiente)
                resumen.Mensaje = "Liquidez insuficiente.";
            else
                resumen.Mensaje = "Compra inmediata disponible.";

            return resumen;
        }

        public async Task<CompraInmediataResponseDto> ConfirmarCompraNormalAsync(
            string tokenSesion,
            ConfirmarCompraInmediataRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.CantidadAObtener <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            var resumen = await ObtenerResumenCompraNormalAsync(
                tokenSesion,
                new CompraInmediataRequestDto
                {
                    ParMonedaId = request.ParMonedaId,
                    CantidadAObtener = request.CantidadAObtener
                });

            if (resumen.CantidadEjecutable <= 0)
                throw new InvalidOperationException("No existe liquidez disponible.");

            if (!resumen.LiquidezSuficiente && !request.ComprarCantidadDisponible)
                throw new InvalidOperationException("No existe suficiente liquidez para cubrir toda la cantidad solicitada.");

            if (!resumen.SaldoSuficiente)
                throw new InvalidOperationException("Saldo insuficiente.");

            var cantidadAEjecutar = request.ComprarCantidadDisponible
                ? resumen.CantidadEjecutable
                : request.CantidadAObtener;

            return await _compraRepository.EjecutarCompraInmediataNormalAsync(
                usuarioId,
                request.ParMonedaId,
                cantidadAEjecutar);
        }

        public async Task<TiempoEstimadoBusquedaRutaDto> ObtenerTiempoEstimadoBusquedaRutaAsync(
            int cantidadMaximaSaltos)
        {
            ValidarCantidadSaltos(cantidadMaximaSaltos);

            var paresActivos = await _compraRepository.ObtenerParesActivosAsync();

            var cantidadMonedas = paresActivos
                .Select(p => p.MonedaOrigenId)
                .Concat(paresActivos.Select(p => p.MonedaDestinoId))
                .Distinct()
                .Count();

            var rutasEstimadas = CalcularRutasEstimadas(cantidadMonedas, cantidadMaximaSaltos);
            var tiempoEstimadoSegundos = CalcularTiempoEstimadoSegundos(rutasEstimadas);
            var tiempoEstimadoMs = (int)Math.Ceiling(tiempoEstimadoSegundos * 1000m);
            var tiempoTexto = FormatearTiempo(tiempoEstimadoSegundos);

            return new TiempoEstimadoBusquedaRutaDto
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

        public async Task<ResultadoBusquedaRutaCompraDto> BuscarMejorRutaAsync(
            string tokenSesion,
            BuscarMejorRutaCompraRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.CantidadAObtener <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            ValidarCantidadSaltos(request.CantidadMaximaSaltos);

            var parObjetivo = await _compraRepository.ObtenerParMonedaAsync(request.ParMonedaId);

            if (parObjetivo == null)
                throw new ArgumentException("El par de monedas no existe.");

            var resumenNormal = await ObtenerResumenCompraNormalAsync(
                tokenSesion,
                new CompraInmediataRequestDto
                {
                    ParMonedaId = request.ParMonedaId,
                    CantidadAObtener = request.CantidadAObtener
                });

            var busqueda = await _compraRepository.CrearBusquedaRutaAsync(
                usuarioId,
                request.ParMonedaId,
                request.CantidadAObtener,
                request.CantidadMaximaSaltos,
                (await ObtenerTiempoEstimadoBusquedaRutaAsync(request.CantidadMaximaSaltos)).TiempoEstimadoMs);

            var pares = await _compraRepository.ObtenerParesActivosAsync();

            var rutas = GenerarRutas(
                parObjetivo.MonedaOrigenId,
                parObjetivo.MonedaDestinoId,
                request.CantidadMaximaSaltos,
                pares);

            ResultadoBusquedaRutaCompraDto? mejorRuta = null;

            foreach (var ruta in rutas)
            {
                var resultadoRuta = await EvaluarRutaAsync(
                    request.CantidadAObtener,
                    ruta,
                    resumenNormal.TotalEstimado);

                if (resultadoRuta == null)
                    continue;

                if (mejorRuta == null || resultadoRuta.TotalRutaEncontrada < mejorRuta.TotalRutaEncontrada)
                    mejorRuta = resultadoRuta;
            }

            if (mejorRuta == null || mejorRuta.TotalRutaEncontrada >= resumenNormal.TotalEstimado)
            {
                await _compraRepository.FinalizarBusquedaRutaSinResultadoAsync(busqueda.BusquedaRutaId);

                return new ResultadoBusquedaRutaCompraDto
                {
                    BusquedaRutaId = busqueda.BusquedaRutaId,
                    ParMonedaId = request.ParMonedaId,
                    MonedaOrigen = ObtenerCodigoMoneda(parObjetivo.MonedaOrigen),
                    MonedaDestino = ObtenerCodigoMoneda(parObjetivo.MonedaDestino),
                    CantidadSolicitada = request.CantidadAObtener,
                    CantidadSaltos = request.CantidadMaximaSaltos,
                    TotalCompraNormal = resumenNormal.TotalEstimado,
                    TotalRutaEncontrada = 0,
                    AhorroEstimado = 0,
                    RutaEncontrada = false,
                    Mensaje = "No se encontró una ruta más barata."
                };
            }

            mejorRuta.BusquedaRutaId = busqueda.BusquedaRutaId;
            mejorRuta.ParMonedaId = request.ParMonedaId;
            mejorRuta.MonedaOrigen = ObtenerCodigoMoneda(parObjetivo.MonedaOrigen);
            mejorRuta.MonedaDestino = ObtenerCodigoMoneda(parObjetivo.MonedaDestino);
            mejorRuta.CantidadSolicitada = request.CantidadAObtener;
            mejorRuta.TotalCompraNormal = resumenNormal.TotalEstimado;
            mejorRuta.AhorroEstimado = resumenNormal.TotalEstimado - mejorRuta.TotalRutaEncontrada;
            mejorRuta.RutaEncontrada = true;
            mejorRuta.Mensaje = "Ruta más barata encontrada.";

            await _compraRepository.GuardarResultadoRutaAsync(
                busqueda.BusquedaRutaId,
                mejorRuta);

            return mejorRuta;
        }

        public async Task<bool> CancelarBusquedaRutaAsync(
            string tokenSesion,
            int busquedaRutaId)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            var busqueda = await _compraRepository.ObtenerBusquedaRutaAsync(busquedaRutaId, usuarioId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            if (busqueda.Estado == "Completada")
                throw new InvalidOperationException("La búsqueda ya fue completada.");

            await _compraRepository.CancelarBusquedaRutaAsync(busquedaRutaId);

            return true;
        }

        public async Task<CompraInmediataResponseDto> ConfirmarCompraPorRutaAsync(
            string tokenSesion,
            ConfirmarCompraRutaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            var busqueda = await _compraRepository.ObtenerBusquedaRutaConSaltosAsync(
                request.BusquedaRutaId,
                usuarioId);

            if (busqueda == null)
                throw new ArgumentException("La búsqueda no existe.");

            if (busqueda.Estado != "Completada")
                throw new InvalidOperationException("La búsqueda no tiene una ruta disponible para confirmar.");

            return await _compraRepository.EjecutarCompraInmediataPorRutaAsync(
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
                throw new InvalidOperationException("Su cuenta se encuentra restringida y no puede realizar compras inmediatas.");

            return sesion.UsuarioId;
        }

        private static void ValidarCantidadSaltos(int cantidadMaximaSaltos)
        {
            if (cantidadMaximaSaltos < 1)
                throw new ArgumentException("Mínimo 1 salto.");

            if (cantidadMaximaSaltos > 5)
                throw new ArgumentException("Máximo 5 saltos.");
        }

        private static ResumenCompraInmediataDto CalcularResumenCompra(
            int parMonedaId,
            string monedaOrigen,
            string monedaDestino,
            decimal cantidadSolicitada,
            List<OfertasVenta> ofertas)
        {
            decimal cantidadPendiente = cantidadSolicitada;
            decimal cantidadEjecutable = 0;
            decimal totalEstimado = 0;

            var preciosUsados = new List<decimal>();

            foreach (var oferta in ofertas.OrderBy(o => o.PrecioUnitario))
            {
                if (cantidadPendiente <= 0)
                    break;

                var cantidadTomada = Math.Min(cantidadPendiente, oferta.CantidadPendiente);
                var subtotal = cantidadTomada * oferta.PrecioUnitario;

                cantidadEjecutable += cantidadTomada;
                totalEstimado += subtotal;
                cantidadPendiente -= cantidadTomada;
                preciosUsados.Add(oferta.PrecioUnitario);
            }

            return new ResumenCompraInmediataDto
            {
                ParMonedaId = parMonedaId,
                MonedaOrigen = monedaOrigen,
                MonedaDestino = monedaDestino,
                CantidadSolicitada = cantidadSolicitada,
                CantidadDisponible = ofertas.Sum(o => o.CantidadPendiente),
                CantidadEjecutable = cantidadEjecutable,
                PrecioMinimoVenta = preciosUsados.Count > 0 ? preciosUsados.Min() : null,
                PrecioMaximoVenta = preciosUsados.Count > 0 ? preciosUsados.Max() : null,
                PrecioPromedioVenta = cantidadEjecutable > 0 ? totalEstimado / cantidadEjecutable : null,
                TotalEstimado = totalEstimado,
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

        private async Task<ResultadoBusquedaRutaCompraDto?> EvaluarRutaAsync(
            decimal cantidadFinalDeseada,
            List<ParesMoneda> ruta,
            decimal totalCompraNormal)
        {
            decimal cantidadNecesaria = cantidadFinalDeseada;
            var saltosInvertidos = new List<SaltoRutaCompraDto>();

            for (int i = ruta.Count - 1; i >= 0; i--)
            {
                var par = ruta[i];

                var ofertas = await _compraRepository.ObtenerOfertasVentaActivasAsync(par.ParMonedaId);

                var resumen = CalcularResumenCompra(
                    par.ParMonedaId,
                    ObtenerCodigoMoneda(par.MonedaOrigen),
                    ObtenerCodigoMoneda(par.MonedaDestino),
                    cantidadNecesaria,
                    ofertas);

                if (!resumen.LiquidezSuficiente)
                    return null;

                saltosInvertidos.Add(new SaltoRutaCompraDto
                {
                    NumeroSalto = i + 1,
                    ParMonedaId = par.ParMonedaId,
                    MonedaOrigen = ObtenerCodigoMoneda(par.MonedaOrigen),
                    MonedaDestino = ObtenerCodigoMoneda(par.MonedaDestino),
                    CantidadConvertida = resumen.TotalEstimado,
                    ResultadoObtenido = cantidadNecesaria,
                    PrecioMinimo = resumen.PrecioMinimoVenta,
                    PrecioMaximo = resumen.PrecioMaximoVenta,
                    PrecioPromedio = resumen.PrecioPromedioVenta
                });

                cantidadNecesaria = resumen.TotalEstimado;
            }

            var saltos = saltosInvertidos
                .OrderBy(s => s.NumeroSalto)
                .ToList();

            return new ResultadoBusquedaRutaCompraDto
            {
                CantidadSaltos = saltos.Count,
                TotalCompraNormal = totalCompraNormal,
                TotalRutaEncontrada = cantidadNecesaria,
                AhorroEstimado = totalCompraNormal - cantidadNecesaria,
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
                PrecioPromedio = cantidadFinalDeseada > 0
                    ? cantidadNecesaria / cantidadFinalDeseada
                    : null,
                RutaEncontrada = cantidadNecesaria < totalCompraNormal,
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