using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using X_Chang.CORE.Core.DTOs.GestionUsuarios;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Settings;

namespace X_Chang.CORE.Infrastructure.Shared
{
    public class GroqAdminMensajeIaService : IAdminMensajeIaService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;

        public GroqAdminMensajeIaService(HttpClient httpClient, IOptions<GroqSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> GenerarMensajeAsync(
            UsuarioAdminDetalleDto usuario,
            string tipoAccion,
            string? mensajeActual,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("La API key de Groq no está configurada en el backend.");

            var accionNormalizada = NormalizarAccion(tipoAccion);
            var textoBase = mensajeActual?.Trim() ?? string.Empty;
            var prompt = ConstruirPrompt(usuario, accionNormalizada, textoBase);

            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var body = new
            {
                model = string.IsNullOrWhiteSpace(_settings.Model)
                    ? "llama-3.1-8b-instant"
                    : _settings.Model,
                temperature = 0.25,
                max_tokens = 120,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "Eres un asistente administrativo para Ezchange. Redactas mensajes breves, profesionales y neutrales en español. No inventes acusaciones, delitos, fraude ni motivos no sustentados. Devuelve solo el mensaje final, sin comillas ni viñetas. Máximo 300 caracteres."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Groq respondió con error {(int)response.StatusCode}: {responseText}");

            using var json = JsonDocument.Parse(responseText);
            var contenido = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;

            return LimpiarMensaje(contenido);
        }

        private static string ConstruirPrompt(
            UsuarioAdminDetalleDto usuario,
            string tipoAccion,
            string mensajeActual)
        {
            var operaciones = usuario.HistorialTransacciones
                .OrderByDescending(h => h.FechaHora)
                .Take(10)
                .Select(h =>
                    $"- {h.FechaHora:yyyy-MM-dd HH:mm}: {h.TipoOperacion}; par={h.ParMoneda ?? "N/A"}; moneda={h.Moneda ?? "N/A"}; estado={h.Estado}; método={h.MetodoEjecucion ?? "N/A"}")
                .DefaultIfEmpty("- Sin operaciones recientes.");

            var restricciones = usuario.HistorialRestricciones
                .OrderByDescending(r => r.FechaInicio)
                .Take(5)
                .Select(r =>
                    $"- {r.FechaInicio:yyyy-MM-dd HH:mm}: {r.TipoAccion}; estado={r.EstadoRestriccion}; mensaje previo={r.Mensaje}")
                .DefaultIfEmpty("- Sin restricciones/habilitaciones previas.");

            var modo = string.IsNullOrWhiteSpace(mensajeActual)
                ? "Genera desde cero un mensaje administrativo."
                : "Mejora el mensaje escrito sin cambiar su intención.";

            return $"""
                Acción solicitada: {tipoAccion}.
                Usuario afectado: {usuario.NombreUsuario}.
                Estado actual: {usuario.Estado}.
                País: {usuario.PaisResidencia}.

                Mensaje actual del administrador:
                {(string.IsNullOrWhiteSpace(mensajeActual) ? "[vacío]" : mensajeActual)}

                Últimas operaciones del usuario:
                {string.Join('\n', operaciones)}

                Historial reciente de restricciones/habilitaciones:
                {string.Join('\n', restricciones)}

                Instrucciones:
                - {modo}
                - Máximo 300 caracteres.
                - Tono claro, profesional y neutral.
                - No inventes acusaciones, fraude, intenciones ni hechos no listados.
                - Para restricción, comunica la medida y que puede retirar fondos disponibles.
                - Para habilitación, comunica que la cuenta vuelve a estar activa.
                - Devuelve solo el mensaje final.
                """;
        }

        private static string NormalizarAccion(string tipoAccion)
        {
            var accion = tipoAccion.Trim();

            return accion.Equals("Habilitación", StringComparison.OrdinalIgnoreCase) ||
                   accion.Equals("Habilitacion", StringComparison.OrdinalIgnoreCase) ||
                   accion.Equals("habilitar", StringComparison.OrdinalIgnoreCase)
                ? "Habilitación"
                : "Restricción";
        }

        private static string LimpiarMensaje(string mensaje)
        {
            var limpio = mensaje
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim()
                .Trim('"', '\'', '“', '”');

            while (limpio.Contains("  "))
                limpio = limpio.Replace("  ", " ");

            if (limpio.Length > 300)
                limpio = limpio[..300].Trim();

            return limpio;
        }
    }
}
