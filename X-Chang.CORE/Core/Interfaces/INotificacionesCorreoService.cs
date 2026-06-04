namespace X_Chang.CORE.Core.Interfaces;

public interface INotificacionesCorreoService
{
    Task ProcesarPendientesAsync();

    Task EncolarAsync(
        int usuarioId,
        string tipoEvento,
        string asunto,
        string cuerpo,
        string? referenciaTipo = null,
        int? referenciaId = null);
}
