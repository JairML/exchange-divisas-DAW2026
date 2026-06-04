using X_Chang.CORE.Core.DTOs.Catalogos;

namespace X_Chang.CORE.Core.Interfaces;

public interface IMetodosPagoRepository
{
    Task<List<MetodoPagoDto>> ObtenerTodosActivosAsync();
    Task<MetodoPagoDto?> ObtenerPorIdAsync(int metodoPagoId);
    Task<List<MetodoPagoDto>> ObtenerParaPaisAsync(int paisId, string[] tipos);
}
