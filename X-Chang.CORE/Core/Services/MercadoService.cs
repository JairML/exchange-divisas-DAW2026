using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class MercadoService : IMercadoService
    {
        private readonly IMercadoRepository _repository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public MercadoService(IMercadoRepository repository, ISesionUsuarioRepository sesionRepository)
        {
            _repository = repository;
            _sesionRepository = sesionRepository;
        }

        public async Task<OperacionesActivasResponseDto> ObtenerOperacionesActivasAsync(string tokenSesion, FiltroOperacionesActivasDto filtro)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: false);
            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            ValidarRegistrosPorPagina(filtro.RegistrosPorPagina, new[] { "10", "20", "50", "100" });
            if (filtro.Pagina < 1) filtro.Pagina = 1;
            return await _repository.ObtenerOperacionesActivasAsync(usuarioId, filtro);
        }

        public Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId, bool verTodasOrdenes, bool verTodasOfertas)
        {
            if (parMonedaId <= 0)
                throw new ArgumentException("El par de monedas no existe.");

            return _repository.ObtenerLibroOrdenesAsync(parMonedaId, verTodasOrdenes, verTodasOfertas);
        }

        public async Task<ResumenOrdenCompraDto> ObtenerResumenOrdenCompraAsync(string tokenSesion, CrearOrdenCompraRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOrden(request);
            return await _repository.ObtenerResumenOrdenCompraAsync(usuarioId, request);
        }

        public async Task<OrdenCompraResultadoDto> CrearOrdenCompraAsync(string tokenSesion, CrearOrdenCompraRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOrden(request);
            return await _repository.CrearOrdenCompraAsync(usuarioId, request);
        }

        public async Task<ResumenOfertaVentaDto> ObtenerResumenOfertaVentaAsync(string tokenSesion, CrearOfertaVentaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOferta(request);
            return await _repository.ObtenerResumenOfertaVentaAsync(usuarioId, request);
        }

        public async Task<OfertaVentaResultadoDto> CrearOfertaVentaAsync(string tokenSesion, CrearOfertaVentaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOferta(request);
            return await _repository.CrearOfertaVentaAsync(usuarioId, request);
        }

        public async Task<PanelAdministrativoDto> ObtenerPanelAdministrativoAsync(string tokenSesion, FiltroPanelAdministrativoDto filtro)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: false);
            var esAdmin = await _repository.EsAdministradorActivoAsync(usuarioId);
            if (!esAdmin)
                throw new UnauthorizedAccessException("Acceso no autorizado");

            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            return await _repository.ObtenerPanelAdministrativoAsync(filtro);
        }

        public async Task<ActividadRecientePaginadaDto> ObtenerActividadRecienteAsync(string tokenSesion, FiltroActividadRecienteDto filtro)
        {
            await ValidarAdminAsync(tokenSesion);
            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            if (filtro.Pagina < 1) filtro.Pagina = 1;
            return await _repository.ObtenerActividadRecienteAsync(filtro);
        }

        public async Task<ExportarPanelAdminResponseDto> ExportarActividadRecienteExcelAsync(string tokenSesion, ExportarPanelAdminRequestDto filtro)
        {
            await ValidarAdminAsync(tokenSesion);
            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            var registros = await _repository.ObtenerActividadRecienteParaExportarAsync(filtro.FechaDesde, filtro.FechaHasta);
            var contenido = ConstruirHtmlExcelActividad(registros);

            return new ExportarPanelAdminResponseDto
            {
                NombreArchivo = $"actividad_reciente_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                TipoContenido = "application/vnd.ms-excel",
                Archivo = System.Text.Encoding.UTF8.GetBytes(contenido)
            };
        }

        public async Task<ExportarPanelAdminResponseDto> ExportarActividadRecientePdfAsync(string tokenSesion, ExportarPanelAdminRequestDto filtro)
        {
            await ValidarAdminAsync(tokenSesion);
            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            var registros = await _repository.ObtenerActividadRecienteParaExportarAsync(filtro.FechaDesde, filtro.FechaHasta);
            var pdf = ConstruirPdfBasicoActividad(registros);

            return new ExportarPanelAdminResponseDto
            {
                NombreArchivo = $"actividad_reciente_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                TipoContenido = "application/pdf",
                Archivo = pdf
            };
        }

        private async Task ValidarAdminAsync(string tokenSesion)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: false);
            var esAdmin = await _repository.EsAdministradorActivoAsync(usuarioId);
            if (!esAdmin)
                throw new UnauthorizedAccessException("Acceso no autorizado");
        }

        private static string ConstruirHtmlExcelActividad(List<ActividadRecienteAdminDto> registros)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");
            sb.AppendLine("<table border=\"1\"><thead><tr>");
            sb.AppendLine("<th>Fecha y hora</th><th>Usuario</th><th>Tipo de operación</th><th>Par</th><th>Monto total</th><th>Estado</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var r in registros)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"))}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Usuario)}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.TipoOperacion)}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Par ?? "")}</td>");
                sb.AppendLine($"<td>{r.MontoTotal}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Estado)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table></body></html>");
            return sb.ToString();
        }

        private static byte[] ConstruirPdfBasicoActividad(List<ActividadRecienteAdminDto> registros)
        {
            var texto = new System.Text.StringBuilder();
            texto.AppendLine("Actividad reciente de la plataforma");
            texto.AppendLine();

            foreach (var r in registros)
            {
                texto.AppendLine($"{r.FechaHora:yyyy-MM-dd HH:mm:ss} | {r.Usuario} | {r.TipoOperacion} | {r.Par} | {r.MontoTotal} | {r.Estado}");
            }

            if (!registros.Any())
                texto.AppendLine("No existen operaciones para el período seleccionado");

            var lineas = texto.ToString()
                .Replace("(", "[")
                .Replace(")", "]")
                .Split('\n')
                .Take(45)
                .ToList();

            var contenido = new System.Text.StringBuilder();
            contenido.AppendLine("BT");
            contenido.AppendLine("/F1 10 Tf");
            contenido.AppendLine("50 780 Td");

            foreach (var linea in lineas)
            {
                contenido.AppendLine($"({linea.Trim()}) Tj");
                contenido.AppendLine("0 -14 Td");
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
<< /Length {System.Text.Encoding.ASCII.GetByteCount(stream)} >>
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
            return System.Text.Encoding.ASCII.GetBytes(pdf);
        }

        private async Task<int> ObtenerUsuarioIdAsync(string tokenSesion, bool bloquearRestringido)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);
            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            if (bloquearRestringido && sesion.Usuario.Estado == "Restringido")
                throw new InvalidOperationException("Su cuenta se encuentra restringida y no puede generar órdenes, ofertas ni operaciones inmediatas.");

            return sesion.UsuarioId;
        }

        private static void ValidarOrden(CrearOrdenCompraRequestDto request)
        {
            if (request.ParMonedaId <= 0)
                throw new ArgumentException("El par de monedas no existe.");
            if (request.CantidadAObtener <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");
            if (request.PrecioUnitario <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0");
        }

        private static void ValidarOferta(CrearOfertaVentaRequestDto request)
        {
            if (request.ParMonedaId <= 0)
                throw new ArgumentException("El par de monedas no existe.");
            if (request.CantidadAVender <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");
            if (request.PrecioUnitario <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0");
        }

        private static void ValidarFechas(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha final debe ser posterior a la fecha inicial");
        }

        private static void ValidarRegistrosPorPagina(string valor, string[] permitidos)
        {
            if (!permitidos.Contains(valor))
                throw new ArgumentException("Cantidad de registros por página inválida.");
        }
    }
}
