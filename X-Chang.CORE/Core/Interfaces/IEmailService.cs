using X_Chang.CORE.Core.Entities;

namespace X_Chang.CORE.Core.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarAsync(string destinatario, string asunto, string cuerpo, IEnumerable<AdjuntosCorreo>? adjuntos = null);
}
