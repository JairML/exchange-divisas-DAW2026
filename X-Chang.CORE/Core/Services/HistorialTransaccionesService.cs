using System.Net;
using System.Text;
using X_Chang.CORE.Core.DTOs.HistorialTransacciones;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class HistorialTransaccionesService : IHistorialTransaccionesService
    {
        private readonly IHistorialTransaccionesRepository _repository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public HistorialTransaccionesService(
            IHistorialTransaccionesRepository repository,
            ISesionUsuarioRepository sesionRepository)
        {
            _repository = repository;
            _sesionRepository = sesionRepository;
        }

        public async Task<HistorialTransaccionesResponseDto> ObtenerHistorialAsync(
            string tokenSesion, HistorialTransaccionesRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);
            ValidarFechas(request.FechaDesde, request.FechaHasta);

            if (request.NumeroPagina < 1)
                request.NumeroPagina = 1;

            if (!EsRegistrosPorPaginaValido(request.RegistrosPorPagina))
                request.RegistrosPorPagina = 5;

            var response = new HistorialTransaccionesResponseDto();
            var cargarTodo = string.IsNullOrWhiteSpace(request.Columna);

            if (cargarTodo || request.Columna == "OrdenesCompra")
                response.OrdenesCompra = await _repository.ObtenerOrdenesCompraAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "OfertasVenta")
                response.OfertasVenta = await _repository.ObtenerOfertasVentaAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "ComprasInmediatas")
                response.ComprasInmediatas = await _repository.ObtenerComprasInmediatasAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "VentasInmediatas")
                response.VentasInmediatas = await _repository.ObtenerVentasInmediatasAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "Depositos")
                response.Depositos = await _repository.ObtenerDepositosAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "Retiros")
                response.Retiros = await _repository.ObtenerRetirosAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            return response;
        }

        public async Task<HistorialTransaccionesResponseDto> ObtenerParaExportarAsync(
            string tokenSesion, DateTime? fechaDesde, DateTime? fechaHasta, string? columna)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);
            ValidarFechas(fechaDesde, fechaHasta);

            var response = new HistorialTransaccionesResponseDto();
            var cargarTodo = string.IsNullOrWhiteSpace(columna);

            if (cargarTodo || columna == "OrdenesCompra")
                response.OrdenesCompra = await _repository.ObtenerOrdenesCompraAsync(usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "OfertasVenta")
                response.OfertasVenta = await _repository.ObtenerOfertasVentaAsync(usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "ComprasInmediatas")
                response.ComprasInmediatas = await _repository.ObtenerComprasInmediatasAsync(usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "VentasInmediatas")
                response.VentasInmediatas = await _repository.ObtenerVentasInmediatasAsync(usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "Depositos")
                response.Depositos = await _repository.ObtenerDepositosAsync(usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "Retiros")
                response.Retiros = await _repository.ObtenerRetirosAsync(usuarioId, fechaDesde, fechaHasta, 1, 0);

            return response;
        }

        public async Task<ExportarHistorialResponseDto> ExportarExcelAsync(
            string tokenSesion, ExportarHistorialRequestDto filtro)
        {
            var historial = await ObtenerParaExportarAsync(
                tokenSesion,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.Columna);

            var contenido = ConstruirHtmlExcel(historial, filtro.Columna);

            return new ExportarHistorialResponseDto
            {
                NombreArchivo = $"historial_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                TipoContenido = "application/vnd.ms-excel",
                Archivo = Encoding.UTF8.GetBytes(contenido)
            };
        }

        public async Task<ExportarHistorialResponseDto> ExportarPdfAsync(
            string tokenSesion, ExportarHistorialRequestDto filtro)
        {
            var historial = await ObtenerParaExportarAsync(
                tokenSesion,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.Columna);

            var texto = ConstruirTextoPdf(historial, filtro.Columna);
            var pdf = ConstruirPdfBasico(texto);

            return new ExportarHistorialResponseDto
            {
                NombreArchivo = $"historial_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                TipoContenido = "application/pdf",
                Archivo = pdf
            };
        }

        private async Task<int> ObtenerUsuarioIdDesdeSesionAsync(string tokenSesion)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);

            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            return sesion.UsuarioId;
        }

        private static void ValidarFechas(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value > fechaHasta.Value)
                throw new ArgumentException("La fecha final debe ser posterior a la fecha inicial.");
        }

        private static bool EsRegistrosPorPaginaValido(int valor)
        {
            return valor is 0 or 5 or 10 or 20 or 40 or 100 or 200;
        }

        private static string ConstruirHtmlExcel(HistorialTransaccionesResponseDto historial, string? columna)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");

            if (DebeIncluir(columna, "OrdenesCompra"))
            {
                AgregarTabla(sb, "Órdenes de compra", new[]
                {
                    "Fecha y hora", "Par", "Cantidad original", "Cantidad obtenida", "Cantidad pendiente", "Precio unitario", "Total comprometido", "Total ejecutado", "Estado"
                }, historial.OrdenesCompra.Lista.Select(r => new[]
                {
                    F(r.FechaHora), r.ParMonedas, N(r.CantidadOriginal), N(r.CantidadObtenida), N(r.CantidadPendiente), N(r.PrecioUnitario), N(r.TotalComprometido), N(r.TotalEjecutado), r.Estado
                }));
            }

            if (DebeIncluir(columna, "OfertasVenta"))
            {
                AgregarTabla(sb, "Ofertas de venta", new[]
                {
                    "Fecha y hora", "Par", "Cantidad original", "Cantidad vendida", "Cantidad pendiente", "Precio unitario", "Total esperado", "Total recibido", "Estado"
                }, historial.OfertasVenta.Lista.Select(r => new[]
                {
                    F(r.FechaHora), r.ParMonedas, N(r.CantidadOriginal), N(r.CantidadVendida), N(r.CantidadPendiente), N(r.PrecioUnitario), N(r.TotalEsperado), N(r.TotalRecibido), r.Estado
                }));
            }

            if (DebeIncluir(columna, "ComprasInmediatas"))
            {
                AgregarTabla(sb, "Compras inmediatas", new[]
                {
                    "Fecha y hora", "Par", "Cantidad obtenida", "Precio mínimo", "Precio máximo", "Precio promedio", "Total pagado", "Estado", "Método"
                }, historial.ComprasInmediatas.Lista.Select(r => new[]
                {
                    F(r.FechaHora), r.ParMonedas, N(r.CantidadObtenida), N(r.PrecioMinCompra), N(r.PrecioMaxCompra), N(r.PrecioPromedioCompra), N(r.TotalPagado), r.Estado, r.MetodoEjecucion
                }));
            }

            if (DebeIncluir(columna, "VentasInmediatas"))
            {
                AgregarTabla(sb, "Ventas inmediatas", new[]
                {
                    "Fecha y hora", "Par", "Cantidad vendida", "Precio mínimo", "Precio máximo", "Precio promedio", "Total recibido", "Estado", "Método"
                }, historial.VentasInmediatas.Lista.Select(r => new[]
                {
                    F(r.FechaHora), r.ParMonedas, N(r.CantidadVendida), N(r.PrecioMinVenta), N(r.PrecioMaxVenta), N(r.PrecioPromedioVenta), N(r.TotalRecibido), r.Estado, r.MetodoEjecucion
                }));
            }

            if (DebeIncluir(columna, "Depositos"))
            {
                AgregarTabla(sb, "Depósitos", new[]
                {
                    "Fecha y hora", "Moneda", "Monto depositado", "Método de pago", "Comisión", "Total pagado", "Estado"
                }, historial.Depositos.Lista.Select(r => new[]
                {
                    F(r.FechaHora), r.Moneda, N(r.MontoDepositado), r.MetodoPago, N(r.ComisionAplicada), N(r.TotalPagado), r.Estado
                }));
            }

            if (DebeIncluir(columna, "Retiros"))
            {
                AgregarTabla(sb, "Retiros", new[]
                {
                    "Fecha y hora", "Moneda", "Monto retirado", "Método de cobro", "Comisión", "Monto final recibido", "Estado"
                }, historial.Retiros.Lista.Select(r => new[]
                {
                    F(r.FechaHora), r.Moneda, N(r.MontoRetirado), r.MetodoCobro, N(r.ComisionAplicada), N(r.MontoFinalRecibido), r.Estado
                }));
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static void AgregarTabla(StringBuilder sb, string titulo, string[] encabezados, IEnumerable<string[]> filas)
        {
            sb.AppendLine($"<h2>{WebUtility.HtmlEncode(titulo)}</h2>");
            sb.AppendLine("<table border=\"1\">");
            sb.AppendLine("<thead><tr>");

            foreach (var encabezado in encabezados)
                sb.AppendLine($"<th>{WebUtility.HtmlEncode(encabezado)}</th>");

            sb.AppendLine("</tr></thead><tbody>");

            foreach (var fila in filas)
            {
                sb.AppendLine("<tr>");
                foreach (var valor in fila)
                    sb.AppendLine($"<td>{WebUtility.HtmlEncode(valor)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table><br />");
        }

        private static string ConstruirTextoPdf(HistorialTransaccionesResponseDto historial, string? columna)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Historial de operaciones");
            sb.AppendLine();

            if (DebeIncluir(columna, "OrdenesCompra"))
            {
                sb.AppendLine("Órdenes de compra");
                foreach (var r in historial.OrdenesCompra.Lista)
                    sb.AppendLine($"{F(r.FechaHora)} | {r.ParMonedas} | {N(r.CantidadOriginal)} | {N(r.PrecioUnitario)} | {r.Estado}");
                sb.AppendLine();
            }

            if (DebeIncluir(columna, "OfertasVenta"))
            {
                sb.AppendLine("Ofertas de venta");
                foreach (var r in historial.OfertasVenta.Lista)
                    sb.AppendLine($"{F(r.FechaHora)} | {r.ParMonedas} | {N(r.CantidadOriginal)} | {N(r.PrecioUnitario)} | {r.Estado}");
                sb.AppendLine();
            }

            if (DebeIncluir(columna, "ComprasInmediatas"))
            {
                sb.AppendLine("Compras inmediatas");
                foreach (var r in historial.ComprasInmediatas.Lista)
                    sb.AppendLine($"{F(r.FechaHora)} | {r.ParMonedas} | {N(r.CantidadObtenida)} | {N(r.TotalPagado)} | {r.Estado}");
                sb.AppendLine();
            }

            if (DebeIncluir(columna, "VentasInmediatas"))
            {
                sb.AppendLine("Ventas inmediatas");
                foreach (var r in historial.VentasInmediatas.Lista)
                    sb.AppendLine($"{F(r.FechaHora)} | {r.ParMonedas} | {N(r.CantidadVendida)} | {N(r.TotalRecibido)} | {r.Estado}");
                sb.AppendLine();
            }

            if (DebeIncluir(columna, "Depositos"))
            {
                sb.AppendLine("Depósitos");
                foreach (var r in historial.Depositos.Lista)
                    sb.AppendLine($"{F(r.FechaHora)} | {r.Moneda} | {N(r.MontoDepositado)} | {r.MetodoPago} | {r.Estado}");
                sb.AppendLine();
            }

            if (DebeIncluir(columna, "Retiros"))
            {
                sb.AppendLine("Retiros");
                foreach (var r in historial.Retiros.Lista)
                    sb.AppendLine($"{F(r.FechaHora)} | {r.Moneda} | {N(r.MontoRetirado)} | {r.MetodoCobro} | {r.Estado}");
            }

            return sb.ToString();
        }

        private static byte[] ConstruirPdfBasico(string texto)
        {
            var lineas = texto
                .Replace("(", "[")
                .Replace(")", "]")
                .Split('\n')
                .Take(45)
                .ToList();

            var contenido = new StringBuilder();
            contenido.AppendLine("BT");
            contenido.AppendLine("/F1 9 Tf");
            contenido.AppendLine("50 780 Td");

            foreach (var linea in lineas)
            {
                contenido.AppendLine($"({linea.Trim()}) Tj");
                contenido.AppendLine("0 -13 Td");
            }

            contenido.AppendLine("ET");

            var stream = contenido.ToString();
            var pdf = $"""
%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>
endobj
4 0 obj
<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
endobj
5 0 obj
<< /Length {Encoding.ASCII.GetByteCount(stream)} >>
stream
{stream}
endstream
endobj
xref
0 6
0000000000 65535 f 
trailer
<< /Root 1 0 R /Size 6 >>
startxref
0
%%EOF
""";

            return Encoding.ASCII.GetBytes(pdf);
        }

        private static bool DebeIncluir(string? columna, string valor)
        {
            return string.IsNullOrWhiteSpace(columna) || columna == valor;
        }

        private static string F(DateTime fecha) => fecha.ToString("yyyy-MM-dd HH:mm:ss");
        private static string N(decimal? valor) => valor.HasValue ? valor.Value.ToString("0.########") : "-";
        private static string N(decimal valor) => valor.ToString("0.########");
    }
}
