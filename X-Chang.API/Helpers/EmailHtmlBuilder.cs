namespace X_Chang.API.Helpers;

/// <summary>
/// Genera el HTML de los correos de transacción con la plantilla corporativa de X-Chang.
/// El logo se lee del disco una sola vez y se cachea como base64 para compatibilidad
/// con clientes de correo que bloquean imágenes externas (Gmail, Outlook).
/// </summary>
public static class EmailHtmlBuilder
{
    private static readonly Lazy<string> _logoTag = new(BuildLogoTag);

    private static string BuildLogoTag()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "email-assets", "logo-xchang.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-assets", "logo-xchang.png"),
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path)) continue;
            var b64 = Convert.ToBase64String(File.ReadAllBytes(path));
            return $"<img src=\"data:image/png;base64,{b64}\" alt=\"X-Chang\" " +
                   "style=\"max-width:200px;height:auto;display:block;margin:0 auto 10px;\" />";
        }

        // Fallback sin imagen
        return "<span style=\"color:#ffffff;font-size:22px;font-weight:800;letter-spacing:-0.5px;\">X-Chang</span>";
    }

    /// <summary>
    /// Construye el HTML completo de un email de transacción.
    /// </summary>
    /// <param name="titulo">Título visible bajo el header (ej. "Confirmación de depósito").</param>
    /// <param name="descripcion">Párrafo introductorio del cuerpo.</param>
    /// <param name="filas">Pares (etiqueta, valor) que se muestran en la tabla de datos.</param>
    public static string Build(
        string titulo,
        string descripcion,
        IEnumerable<(string Label, string Value)> filas)
    {
        var rowsHtml = string.Empty;
        var isAlt = false;
        foreach (var (label, value) in filas)
        {
            var bg = isAlt ? "#f8fafc" : "#ffffff";
            rowsHtml += $"""
                <tr style="background:{bg};">
                  <td style="padding:13px 16px;font-size:14px;color:#334155;border-bottom:1px solid #f1f5f9;">{label}</td>
                  <td style="padding:13px 16px;font-size:14px;font-weight:600;color:#0f172a;text-align:right;border-bottom:1px solid #f1f5f9;font-variant-numeric:tabular-nums;">{value}</td>
                </tr>
                """;
            isAlt = !isAlt;
        }

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width,initial-scale=1.0">
              <title>{titulo}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f0f4f8;font-family:Arial,Helvetica,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f0f4f8;padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0"
                           style="max-width:600px;width:100%;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(15,23,42,0.10);">

                      <!-- HEADER azul degradado -->
                      <tr>
                        <td style="background:linear-gradient(135deg,#0d1b3e 0%,#1a306e 55%,#2563eb 100%);padding:30px 32px;text-align:center;">
                          <h1 style="margin:0 0 6px;color:#ffffff;font-size:26px;font-weight:800;letter-spacing:-0.5px;">X-Chang</h1>
                          <p style="margin:0;color:rgba(255,255,255,0.70);font-size:13px;letter-spacing:0.02em;">{titulo}</p>
                        </td>
                      </tr>

                      <!-- CUERPO -->
                      <tr>
                        <td style="padding:32px;">
                          <p style="margin:0 0 24px;color:#334155;font-size:15px;line-height:1.6;">{descripcion}</p>

                          <!-- Tabla de datos con filas alternadas -->
                          <table width="100%" cellpadding="0" cellspacing="0"
                                 style="border-collapse:collapse;border:1px solid #e2e8f0;border-radius:10px;overflow:hidden;">
                            <thead>
                              <tr style="background:#0d1b3e;">
                                <th style="padding:11px 16px;font-size:11px;font-weight:700;color:#93c5fd;text-transform:uppercase;letter-spacing:0.07em;text-align:left;border-bottom:2px solid #2563eb;">Concepto</th>
                                <th style="padding:11px 16px;font-size:11px;font-weight:700;color:#93c5fd;text-transform:uppercase;letter-spacing:0.07em;text-align:right;border-bottom:2px solid #2563eb;">Valor</th>
                              </tr>
                            </thead>
                            <tbody>
                              {rowsHtml}
                            </tbody>
                          </table>

                          <p style="margin:24px 0 0;padding:13px 16px;background:#dbeafe;border-left:4px solid #2563eb;border-radius:8px;font-size:13px;color:#1e40af;line-height:1.5;">
                            Si no reconoces esta operación, contáctanos de inmediato a través de tu cuenta en X-Chang.
                          </p>
                        </td>
                      </tr>

                      <!-- FOOTER oscuro con logo -->
                      <tr>
                        <td style="background:#0d1b3e;padding:24px 32px;text-align:center;">
                          {_logoTag.Value}
                          <p style="margin:10px 0 4px;font-size:13px;font-weight:700;color:#ffffff;">El equipo de X-Chang</p>
                          <p style="margin:0;font-size:12px;color:#64748b;line-height:1.6;">
                            Este correo fue generado automáticamente. Por favor no respondas a este mensaje.
                          </p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }
}
