using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IOfertaService
    {
        Task<OfertaDto> CrearOfertaVentaAsync(int usuarioId, CrearOfertaRequest request);
    }
}