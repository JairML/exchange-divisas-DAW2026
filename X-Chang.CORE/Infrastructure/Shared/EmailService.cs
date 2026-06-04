using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using X_Chang.CORE.Core.Entities;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.Core.Settings;

namespace X_Chang.CORE.Infrastructure.Shared;

// US-018: envío de correos vía SMTP usando MailKit.
public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtp;

    public EmailService(IOptions<SmtpSettings> smtp)
    {
        _smtp = smtp.Value;
    }

    public async Task<bool> EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpo,
        IEnumerable<AdjuntosCorreo>? adjuntos = null)
    {
        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_smtp.NombreRemitente, _smtp.CorreoRemitente));
            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;

            var builder = new BodyBuilder { HtmlBody = ConstruirHtml(cuerpo, adjuntos) };
            mensaje.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var socketOptions = _smtp.UsarSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_smtp.Host, _smtp.Puerto, socketOptions);
            await client.AuthenticateAsync(_smtp.Usuario, _smtp.Password);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(quit: true);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ConstruirHtml(string cuerpo, IEnumerable<AdjuntosCorreo>? adjuntos)
    {
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
