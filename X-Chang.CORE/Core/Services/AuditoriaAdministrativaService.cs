using System;
using System.Collections.Generic;
using System.Text;
using System.Text;
using X_Chang.CORE.Core.DTOs.AuditoriaAdministrativa;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class AuditoriaAdministrativaService : IAuditoriaAdministrativaService
    {
        private readonly IAuditoriaAdministrativaRepository _repository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public AuditoriaAdministrativaService(
            IAuditoriaAdministrativaRepository repository,
            ISesionUsuarioRepository sesionRepository)
        {
            _repository = repository;
            _sesionRepository = sesionRepository;
        }

        public async Task<AuditoriaAdminPaginadoDto> BuscarAuditoriaAsync(
            string tokenSesion,
            FiltroAuditoriaAdminDto filtro)
        {
            await ValidarAdministradorAsync(tokenSesion);
            ValidarFiltro(filtro.FechaDesde, filtro.FechaHasta, filtro.TipoAccion);

            if (filtro.Pagina < 1)
                filtro.Pagina = 1;

            if (!EsCantidadPaginaValida(filtro.RegistrosPorPagina))
                throw new ArgumentException("Cantidad de registros por página inválida.");

            return await _repository.BuscarAuditoriaAsync(filtro);
        }

        public async Task<ExportarAuditoriaResponseDto> ExportarExcelAsync(
            string tokenSesion,
            ExportarAuditoriaRequestDto filtro)
        {
            await ValidarAdministradorAsync(tokenSesion);
            ValidarFiltro(filtro.FechaDesde, filtro.FechaHasta, filtro.TipoAccion);

            var registros = await _repository.BuscarAuditoriaParaExportarAsync(filtro);

            var contenido = ConstruirCsv(registros);

            return new ExportarAuditoriaResponseDto
            {
                NombreArchivo = $"auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                TipoContenido = "text/csv",
                Archivo = Encoding.UTF8.GetBytes(contenido)
            };
        }

        public async Task<ExportarAuditoriaResponseDto> ExportarPdfAsync(
            string tokenSesion,
            ExportarAuditoriaRequestDto filtro)
        {
            await ValidarAdministradorAsync(tokenSesion);
            ValidarFiltro(filtro.FechaDesde, filtro.FechaHasta, filtro.TipoAccion);

            var registros = await _repository.BuscarAuditoriaParaExportarAsync(filtro);

            var texto = ConstruirTextoPdf(registros);
            var pdf = ConstruirPdfBasico(texto);

            return new ExportarAuditoriaResponseDto
            {
                NombreArchivo = $"auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                TipoContenido = "application/pdf",
                Archivo = pdf
            };
        }

        private async Task<int> ValidarAdministradorAsync(string tokenSesion)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);

            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            var esAdmin = await _repository.EsAdministradorActivoAsync(sesion.UsuarioId);

            if (!esAdmin)
                throw new UnauthorizedAccessException("El usuario no tiene permisos de administrador.");

            return sesion.UsuarioId;
        }

        private static void ValidarFiltro(DateTime? fechaDesde, DateTime? fechaHasta, string? tipoAccion)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value > fechaHasta.Value)
                throw new ArgumentException("La fecha final debe ser posterior a la fecha inicial");

            var tipo = tipoAccion?.Trim() ?? "Todos";

            if (tipo != "Todos" && tipo != "Restricción" && tipo != "Habilitación")
                throw new ArgumentException("Tipo de acción inválido.");
        }

        private static bool EsCantidadPaginaValida(string valor)
        {
            var permitidos = new[] { "10", "20", "40", "100", "200", "400", "Todos" };
            return permitidos.Contains(valor);
        }

        private static string ConstruirCsv(List<AuditoriaAdminRegistroDto> registros)
        {
            var sb = new StringBuilder();

            sb.AppendLine("FechaHora,Administrador,UsuarioAfectado,TipoAccion,MensajeRegistrado");

            foreach (var r in registros)
            {
                sb.AppendLine(
                    $"{EscaparCsv(r.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"))}," +
                    $"{EscaparCsv(r.Administrador)}," +
                    $"{EscaparCsv(r.UsuarioAfectado)}," +
                    $"{EscaparCsv(r.TipoAccion)}," +
                    $"{EscaparCsv(r.MensajeRegistrado)}");
            }

            return sb.ToString();
        }

        private static string EscaparCsv(string valor)
        {
            return "\"" + valor.Replace("\"", "\"\"") + "\"";
        }

        private static string ConstruirTextoPdf(List<AuditoriaAdminRegistroDto> registros)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Log de auditoría administrativa");
            sb.AppendLine();

            foreach (var r in registros)
            {
                sb.AppendLine($"{r.FechaHora:yyyy-MM-dd HH:mm:ss} | {r.Administrador} | {r.UsuarioAfectado} | {r.TipoAccion}");
                sb.AppendLine(r.MensajeRegistrado);
                sb.AppendLine();
            }

            if (!registros.Any())
                sb.AppendLine("No se encontraron registros de auditoría");

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
    }
}