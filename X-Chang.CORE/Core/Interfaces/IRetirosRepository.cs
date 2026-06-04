using X_Chang.CORE.Core.DTOs.Billetera;

namespace X_Chang.CORE.Core.Interfaces;

public interface IRetirosRepository
{
    Task<List<DetalleRetiroDto>> ListarAsync(int usuarioId, int pagina, int tamano);
    Task<int> ContarAsync(int usuarioId);
    Task<DetalleRetiroDto?> ObtenerDetalleAsync(int usuarioId, int retiroId);
    Task<DetalleRetiroDto> RegistrarRetiroAsync(int usuarioId, RetirarDto dto);
}
