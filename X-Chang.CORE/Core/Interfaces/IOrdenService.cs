using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IOrdenService
    {
        Task<OrdenDto> CrearOrdenCompraAsync(int usuarioId, CrearOrdenRequest request);
        Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId);
        Task<LibroOrdenesDetalleDto> ObtenerLibroOrdenesDetalleAsync(int parMonedaId, int limite = 10);
    }
}