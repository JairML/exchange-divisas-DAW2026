namespace X_Chang.CORE.Core.Settings;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; }
    public bool UsarSsl { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = string.Empty;
    public string CorreoRemitente { get; set; } = string.Empty;
}
