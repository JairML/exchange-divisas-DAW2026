using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Settings;

namespace X_Chang.CORE.Infrastructure.Shared;

public class EmailService : IEmailService
{
    private const string RutaEnvioCorreo = "v3/smtp/email";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly BrevoSettings _brevo;
    private readonly ILogger<EmailService> _logger;

    public EmailService(HttpClient httpClient, IOptions<BrevoSettings> brevo, ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _brevo = brevo.Value;
        _logger = logger;
    }

    public async Task<bool> EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpo,
        IEnumerable<AdjuntosCorreo>? adjuntos = null)
    {
        _logger.LogInformation("Enviando email a {Destinatario} | Asunto: {Asunto} via Brevo API", destinatario, asunto);

        var payload = new
        {
            sender = new { name = _brevo.NombreRemitente, email = _brevo.CorreoRemitente },
            to = new[] { new { email = destinatario } },
            subject = asunto,
            htmlContent = ConstruirHtml(cuerpo, adjuntos)
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, RutaEnvioCorreo)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("api-key", _brevo.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email enviado exitosamente a {Destinatario}", destinatario);
                return true;
            }

            var contenidoError = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Error al enviar email a {Destinatario} via Brevo API | Status: {StatusCode} | Respuesta: {Respuesta}",
                destinatario, (int)response.StatusCode, contenidoError);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al enviar email a {Destinatario} | {ExceptionType}: {Message}",
                destinatario, ex.GetType().Name, ex.Message);
            return false;
        }
    }

    private static string ConstruirHtml(string cuerpo, IEnumerable<AdjuntosCorreo>? adjuntos)
    {
        // Si el cuerpo ya es HTML completo, lo usamos directamente
        if (cuerpo.TrimStart().StartsWith("<"))
            return cuerpo;

        var html = $"<p>{cuerpo}</p>";

        var lista = adjuntos?.ToList();
        if (lista?.Count > 0)
        {
            html += "<br/><p><strong>Documentos adjuntos:</strong></p><ul>";
            foreach (var adj in lista)
                html += $"<li><a href=\"{adj.UrlArchivo}\">{adj.NombreArchivo}</a></li>";
            html += "</ul>";
        }

        return html;
    }
}
