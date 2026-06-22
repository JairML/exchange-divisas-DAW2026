using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Precios;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class PreciosParService : IPreciosParService
    {
        private readonly IPreciosParRepository _repo;

        // Las 27 monedas soportadas, ordenadas alfabéticamente para generación de pares canónicos.
        private static readonly string[] MonedasSoportadas =
        {
            "ARS", "AUD", "BOB", "BRL", "CAD", "CHF", "CLP", "CNY", "COP", "CRC",
            "CUP", "DOP", "EUR", "GBP", "GTQ", "HKD", "HNL", "JPY", "MXN", "NIO",
            "PAB", "PEN", "PYG", "RUB", "USD", "UYU", "VES"
        };

        public PreciosParService(IPreciosParRepository repo)
        {
            _repo = repo;
        }

        // ─── US-009 ──────────────────────────────────────────────────────────────

        public async Task<MenuPrincipalResponseDto> ObtenerDatosMenuPrincipalAsync(int? usuarioId)
        {
            if (usuarioId == null)
            {
                var serie = await ObtenerSeriePorCodigos("USD", "EUR", null);
                return new MenuPrincipalResponseDto
                {
                    UsuarioAutenticado = false,
                    GraficoPrincipal = new GraficoPreciosParDto
                    {
                        MonedaOrigen = "USD",
                        MonedaDestino = "EUR",
                        Serie = serie
                    }
                };
            }

            // Moneda principal según país de residencia
            var monedaPrincipal = await _repo.ObtenerMonedaPrincipalUsuarioAsync(usuarioId.Value) ?? "USD";

            string origenPrincipal, destinoPrincipal;
            if (monedaPrincipal != "USD")
            {
                origenPrincipal = monedaPrincipal;
                destinoPrincipal = "USD";
            }
            else
            {
                origenPrincipal = "USD";
                destinoPrincipal = "EUR";
            }

            var seriePrincipal = await ObtenerSeriePorCodigos(origenPrincipal, destinoPrincipal, null);
            var graficoPrincipal = new GraficoPreciosParDto
            {
                MonedaOrigen = origenPrincipal,
                MonedaDestino = destinoPrincipal,
                Serie = seriePrincipal
            };

            // Segundo gráfico: par de la orden/oferta activa más reciente del usuario
            var parRecienteId = await _repo.ObtenerParMasRecienteActivoUsuarioAsync(usuarioId.Value);

            string origenSecundario, destinoSecundario;
            if (parRecienteId.HasValue)
            {
                var isos = await _repo.ObtenerIsosPorParIdAsync(parRecienteId.Value);
                if (isos.HasValue)
                {
                    origenSecundario = isos.Value.OrigenIso;
                    destinoSecundario = isos.Value.DestinoIso;
                }
                else
                {
                    // Fallback: primer par invertido
                    origenSecundario = destinoPrincipal;
                    destinoSecundario = origenPrincipal;
                }
            }
            else
            {
                // Sin órdenes/ofertas activas → mismo par del primero pero invertido
                origenSecundario = destinoPrincipal;
                destinoSecundario = origenPrincipal;
            }

            var serieSecundaria = await ObtenerSeriePorCodigos(origenSecundario, destinoSecundario, null);
            var graficoSecundario = new GraficoPreciosParDto
            {
                MonedaOrigen = origenSecundario,
                MonedaDestino = destinoSecundario,
                Serie = serieSecundaria
            };

            return new MenuPrincipalResponseDto
            {
                UsuarioAutenticado = true,
                GraficoPrincipal = graficoPrincipal,
                GraficoSecundario = graficoSecundario
            };
        }

        // ─── US-010 ──────────────────────────────────────────────────────────────

        public async Task<ParesMonedaPaginadoDto> ObtenerListadoParesAsync(
            int? usuarioId, FiltroParesMonedaDto filtro)
        {
            // Datos de referencia desde la BD
            var monedas = await _repo.ObtenerMonedasSoportadasAsync(MonedasSoportadas);
            var monedasPorIso = monedas.ToDictionary(m => m.CodigoIso, m => m.MonedaId);

            var pares = await _repo.ObtenerTodosParesAsync();
            var paresPorIds = pares.ToDictionary(
                p => (p.MonedaOrigenId, p.MonedaDestinoId),
                p => p.ParMonedaId);

            var preciosCompra = await _repo.ObtenerMayoresPreciosCompraAsync();
            var preciosVenta = await _repo.ObtenerMenoresPreciosVentaAsync();

            Dictionary<int, decimal>? volumenes = null;
            if (filtro.Criterio == "Volumen")
                volumenes = await _repo.ObtenerVolumenesPorParAsync();

            Dictionary<int, DateTime>? transacciones = null;
            if (filtro.Criterio == "FechaReciente" && usuarioId.HasValue)
                transacciones = await _repo.ObtenerFechaTransaccionPorParUsuarioAsync(usuarioId.Value);

            // Códigos presentes en la BD (subconjunto de los 27 soportados)
            var codesDisponibles = MonedasSoportadas
                .Where(iso => monedasPorIso.ContainsKey(iso))
                .ToArray(); // ya ordenado alfabéticamente

            // Construir universo de pares
            var todos = filtro.ColapsarParesInversos
                ? BuildPares351(codesDisponibles, monedasPorIso, paresPorIds, preciosCompra, preciosVenta)
                : BuildPares702(codesDisponibles, monedasPorIso, paresPorIds, preciosCompra, preciosVenta);

            // Filtrar
            if (filtro.MonedaEntrega != "Cualquiera")
                todos = todos.Where(p => p.MonedaEntrega == filtro.MonedaEntrega).ToList();

            if (filtro.MonedaObtiene != "Cualquiera")
                todos = todos.Where(p => p.MonedaObtiene == filtro.MonedaObtiene).ToList();

            // Ordenar
            todos = Ordenar(todos, filtro, monedasPorIso, paresPorIds, volumenes, transacciones);

            // Paginar
            var totalRegistros = todos.Count;
            int regPorPag;
            bool esTodos;

            if (string.Equals(filtro.RegistrosPorPagina, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                regPorPag = totalRegistros == 0 ? 1 : totalRegistros;
                esTodos = true;
            }
            else if (!int.TryParse(filtro.RegistrosPorPagina, out regPorPag) || regPorPag <= 0)
            {
                regPorPag = 20;
                esTodos = false;
            }
            else
            {
                esTodos = false;
            }

            var totalPaginas = (int)Math.Ceiling(totalRegistros / (decimal)regPorPag);
            if (totalPaginas < 1) totalPaginas = 1;

            var pagina = Math.Clamp(filtro.Pagina, 1, totalPaginas);
            var registros = esTodos
                ? todos
                : todos.Skip((pagina - 1) * regPorPag).Take(regPorPag).ToList();

            return new ParesMonedaPaginadoDto
            {
                Registros = registros,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                RegistrosPorPagina = filtro.RegistrosPorPagina,
                TienePaginaAnterior = pagina > 1,
                TienePaginaSiguiente = pagina < totalPaginas
            };
        }

        // ─── US-011 ──────────────────────────────────────────────────────────────

        public async Task<ResultadoOperacion<SerieHistoricaParResponseDto>> ObtenerSerieHistoricaAsync(
            string monedaOrigen, string monedaDestino, string rango)
        {
            var origenUpper = monedaOrigen.ToUpperInvariant();
            var destinoUpper = monedaDestino.ToUpperInvariant();

            if (origenUpper == destinoUpper)
                return ResultadoOperacion<SerieHistoricaParResponseDto>.Error(
                    "Las monedas de origen y destino deben ser distintas.");

            var monedas = await _repo.ObtenerMonedasSoportadasAsync(new[] { origenUpper, destinoUpper });
            var origen = monedas.FirstOrDefault(m => m.CodigoIso == origenUpper);
            var destino = monedas.FirstOrDefault(m => m.CodigoIso == destinoUpper);

            if (origen == null || destino == null)
                return ResultadoOperacion<SerieHistoricaParResponseDto>.Error(
                    "Moneda no reconocida o no soportada.");

            var parId = await _repo.ObtenerParMonedaIdAsync(origen.MonedaId, destino.MonedaId);

            // Precios actuales del order book vivo
            var preciosCompra = await _repo.ObtenerMayoresPreciosCompraAsync();
            var preciosVenta = await _repo.ObtenerMenoresPreciosVentaAsync();

            decimal? mayorActual = parId.HasValue && preciosCompra.TryGetValue(parId.Value, out var pc) ? pc : null;
            decimal? menorActual = parId.HasValue && preciosVenta.TryGetValue(parId.Value, out var pv) ? pv : null;
            decimal? margenActual = mayorActual.HasValue && menorActual.HasValue
                ? mayorActual.Value - menorActual.Value
                : null;

            var desde = ComputarDesde(rango);
            var serie = parId.HasValue
                ? await _repo.ObtenerSerieHistoricaAsync(parId.Value, desde)
                : new List<PuntoSerieHistoricaDto>();

            return ResultadoOperacion<SerieHistoricaParResponseDto>.Ok(new SerieHistoricaParResponseDto
            {
                MonedaOrigen = origen.CodigoIso,
                MonedaDestino = destino.CodigoIso,
                Rango = rango,
                MayorPrecioCompraActual = mayorActual,
                MenorPrecioVentaActual = menorActual,
                MargenActual = margenActual,
                Serie = serie
            });
        }

        // ─── Helpers privados ────────────────────────────────────────────────────

        private async Task<List<PuntoSerieHistoricaDto>> ObtenerSeriePorCodigos(
            string origenIso, string destinoIso, DateTime? desde)
        {
            var monedas = await _repo.ObtenerMonedasSoportadasAsync(new[] { origenIso, destinoIso });
            var origen = monedas.FirstOrDefault(m => m.CodigoIso == origenIso);
            var destino = monedas.FirstOrDefault(m => m.CodigoIso == destinoIso);

            if (origen == null || destino == null)
                return new List<PuntoSerieHistoricaDto>();

            var parId = await _repo.ObtenerParMonedaIdAsync(origen.MonedaId, destino.MonedaId);
            if (!parId.HasValue)
                return new List<PuntoSerieHistoricaDto>();

            return await _repo.ObtenerSerieHistoricaAsync(parId.Value, desde);
        }

        private static List<ParMonedaListadoDto> BuildPares702(
            string[] codes,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, decimal> preciosCompra,
            Dictionary<int, decimal> preciosVenta)
        {
            var result = new List<ParMonedaListadoDto>(codes.Length * (codes.Length - 1));
            foreach (var origen in codes)
                foreach (var destino in codes)
                {
                    if (origen == destino) continue;
                    result.Add(BuildDto(origen, destino, monedasPorIso, paresPorIds, preciosCompra, preciosVenta));
                }
            return result;
        }

        private static List<ParMonedaListadoDto> BuildPares351(
            string[] codes,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, decimal> preciosCompra,
            Dictionary<int, decimal> preciosVenta)
        {
            var result = new List<ParMonedaListadoDto>(codes.Length * (codes.Length - 1) / 2);
            for (int i = 0; i < codes.Length; i++)
            {
                for (int j = i + 1; j < codes.Length; j++)
                {
                    // Forma canónica: codes[i] < codes[j] (ya ordenado)
                    var a = codes[i];
                    var b = codes[j];

                    // Precios del par canónico (a→b), con fallback al inverso (b→a)
                    decimal? mayorCompra = null, menorVenta = null;

                    if (monedasPorIso.TryGetValue(a, out var aId) && monedasPorIso.TryGetValue(b, out var bId))
                    {
                        if (paresPorIds.TryGetValue((aId, bId), out var parAb))
                        {
                            if (preciosCompra.TryGetValue(parAb, out var cAb)) mayorCompra = cAb;
                            if (preciosVenta.TryGetValue(parAb, out var vAb)) menorVenta = vAb;
                        }

                        if (mayorCompra == null && menorVenta == null &&
                            paresPorIds.TryGetValue((bId, aId), out var parBa))
                        {
                            if (preciosCompra.TryGetValue(parBa, out var cBa)) mayorCompra = cBa;
                            if (preciosVenta.TryGetValue(parBa, out var vBa)) menorVenta = vBa;
                        }
                    }

                    decimal? margen = mayorCompra.HasValue && menorVenta.HasValue
                        ? mayorCompra.Value - menorVenta.Value
                        : null;

                    result.Add(new ParMonedaListadoDto
                    {
                        MonedaEntrega = a,
                        MonedaObtiene = b,
                        MayorPrecioCompra = mayorCompra,
                        MenorPrecioVenta = menorVenta,
                        Margen = margen
                    });
                }
            }
            return result;
        }

        private static ParMonedaListadoDto BuildDto(
            string origen, string destino,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, decimal> preciosCompra,
            Dictionary<int, decimal> preciosVenta)
        {
            decimal? mayorCompra = null, menorVenta = null;

            if (monedasPorIso.TryGetValue(origen, out var origenId) &&
                monedasPorIso.TryGetValue(destino, out var destinoId) &&
                paresPorIds.TryGetValue((origenId, destinoId), out var parId))
            {
                if (preciosCompra.TryGetValue(parId, out var c)) mayorCompra = c;
                if (preciosVenta.TryGetValue(parId, out var v)) menorVenta = v;
            }

            decimal? margen = mayorCompra.HasValue && menorVenta.HasValue
                ? mayorCompra.Value - menorVenta.Value
                : null;

            return new ParMonedaListadoDto
            {
                MonedaEntrega = origen,
                MonedaObtiene = destino,
                MayorPrecioCompra = mayorCompra,
                MenorPrecioVenta = menorVenta,
                Margen = margen
            };
        }

        private static List<ParMonedaListadoDto> Ordenar(
            List<ParMonedaListadoDto> lista,
            FiltroParesMonedaDto filtro,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, decimal>? volumenes,
            Dictionary<int, DateTime>? transacciones)
        {
            bool desc = filtro.Direccion == "desc";
            bool collapsed = filtro.ColapsarParesInversos;

            switch (filtro.Criterio)
            {
                case "FechaReciente":
                    return OrdenarPorFechaReciente(lista, desc, collapsed, monedasPorIso, paresPorIds, transacciones);

                case "Volumen":
                    var volPorPar = lista
                        .Select(p => (p, vol: GetVolumen(p, collapsed, monedasPorIso, paresPorIds, volumenes)))
                        .ToList();
                    return desc
                        ? volPorPar.OrderByDescending(x => x.vol)
                                   .ThenBy(x => x.p.MonedaEntrega).ThenBy(x => x.p.MonedaObtiene)
                                   .Select(x => x.p).ToList()
                        : volPorPar.OrderBy(x => x.vol)
                                   .ThenBy(x => x.p.MonedaEntrega).ThenBy(x => x.p.MonedaObtiene)
                                   .Select(x => x.p).ToList();

                case "MayorPrecioCompra":
                    return desc
                        ? lista.OrderByDescending(p => p.MayorPrecioCompra ?? decimal.MinValue)
                               .ThenBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList()
                        : lista.OrderBy(p => p.MayorPrecioCompra ?? decimal.MaxValue)
                               .ThenBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList();

                case "MenorPrecioVenta":
                    return desc
                        ? lista.OrderByDescending(p => p.MenorPrecioVenta ?? decimal.MinValue)
                               .ThenBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList()
                        : lista.OrderBy(p => p.MenorPrecioVenta ?? decimal.MaxValue)
                               .ThenBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList();

                case "Margen":
                    return desc
                        ? lista.OrderByDescending(p => p.Margen ?? decimal.MinValue)
                               .ThenBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList()
                        : lista.OrderBy(p => p.Margen ?? decimal.MaxValue)
                               .ThenBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList();

                default:
                    return lista.OrderBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList();
            }
        }

        // Los pares del usuario SIEMPRE van primero (asc o desc solo afecta al orden interno
        // de ese grupo); el resto se ordena siempre alfabético.
        private static List<ParMonedaListadoDto> OrdenarPorFechaReciente(
            List<ParMonedaListadoDto> lista,
            bool desc,
            bool collapsed,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, DateTime>? transacciones)
        {
            if (transacciones == null || transacciones.Count == 0)
                return lista.OrderBy(p => p.MonedaEntrega).ThenBy(p => p.MonedaObtiene).ToList();

            // Precomputa la fecha máxima por par (teniendo en cuenta el par inverso si collapsed)
            var conFecha = lista
                .Select(p => (par: p, fecha: GetFechaTransaccion(p, collapsed, monedasPorIso, paresPorIds, transacciones)))
                .ToList();

            var delUsuario = conFecha.Where(x => x.fecha.HasValue).ToList();
            var restantes = conFecha.Where(x => !x.fecha.HasValue)
                .OrderBy(x => x.par.MonedaEntrega)
                .ThenBy(x => x.par.MonedaObtiene)
                .Select(x => x.par)
                .ToList();

            var sortedUsuario = desc
                ? delUsuario.OrderByDescending(x => x.fecha!.Value).Select(x => x.par).ToList()
                : delUsuario.OrderBy(x => x.fecha!.Value).Select(x => x.par).ToList();

            return sortedUsuario.Concat(restantes).ToList();
        }

        private static DateTime? GetFechaTransaccion(
            ParMonedaListadoDto par,
            bool collapsed,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, DateTime> transacciones)
        {
            if (!monedasPorIso.TryGetValue(par.MonedaEntrega, out var origenId) ||
                !monedasPorIso.TryGetValue(par.MonedaObtiene, out var destinoId))
                return null;

            DateTime? resultado = null;

            if (paresPorIds.TryGetValue((origenId, destinoId), out var parId) &&
                transacciones.TryGetValue(parId, out var fecha))
                resultado = fecha;

            // Para pares colapsados también revisar la dirección inversa
            if (collapsed &&
                paresPorIds.TryGetValue((destinoId, origenId), out var parInvId) &&
                transacciones.TryGetValue(parInvId, out var fechaInv))
            {
                if (resultado == null || fechaInv > resultado.Value)
                    resultado = fechaInv;
            }

            return resultado;
        }

        private static decimal GetVolumen(
            ParMonedaListadoDto par,
            bool collapsed,
            Dictionary<string, int> monedasPorIso,
            Dictionary<(int, int), int> paresPorIds,
            Dictionary<int, decimal>? volumenes)
        {
            if (volumenes == null) return 0m;

            decimal vol = 0m;

            if (monedasPorIso.TryGetValue(par.MonedaEntrega, out var origenId) &&
                monedasPorIso.TryGetValue(par.MonedaObtiene, out var destinoId))
            {
                if (paresPorIds.TryGetValue((origenId, destinoId), out var parId) &&
                    volumenes.TryGetValue(parId, out var v))
                    vol += v;

                if (collapsed &&
                    paresPorIds.TryGetValue((destinoId, origenId), out var parInvId) &&
                    volumenes.TryGetValue(parInvId, out var vInv))
                    vol += vInv;
            }

            return vol;
        }

        private static DateTime? ComputarDesde(string rango) => rango switch
        {
            "UltimoDia"    => DateTime.UtcNow.AddDays(-1),
            "UltimaSemana" => DateTime.UtcNow.AddDays(-7),
            "UltimoMes"    => DateTime.UtcNow.AddMonths(-1),
            "UltimoAno"    => DateTime.UtcNow.AddYears(-1),
            "Total"        => null,
            _              => DateTime.UtcNow.AddDays(-1)   // default: último día
        };
    }
}
