using X_Chang.CORE.Core.DTOs.Billetera;

namespace X_Chang.CORE.Core.Interfaces;

public interface IDepositosRepository
{
    Task<List<DetalleDepositoDto>> ListarAsync(int usuarioId, int pagina, int tamano);
    Task<int> ContarAsync(int usuarioId);
    Task<DetalleDepositoDto?> ObtenerDetalleAsync(int usuarioId, int depositoId);
}
