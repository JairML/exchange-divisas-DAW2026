using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Billetera;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class RetirosService : IRetirosService
{
    private readonly IRetirosRepository _repo;
    private readonly ISesionUsuarioRepository _sesionRepo;
    private readonly IUsuarioRepository _usuarioRepo;

    public RetirosService(
        IRetirosRepository repo,
        ISesionUsuarioRepository sesionRepo,
        IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _sesionRepo = sesionRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<ResultadoOperacion<(List<DetalleRetiroDto> items, int total)>> ListarAsync(
        string tokenSesion, int pagina, int tamano)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<(List<DetalleRetiroDto>, int)>.Error("Sesión inválida o expirada.");

        var items = await _repo.ListarAsync(uid.Value, pagina, tamano);
        var total = await _repo.ContarAsync(uid.Value);
        return ResultadoOperacion<(List<DetalleRetiroDto>, int)>.Ok((items, total));
    }

    public async Task<ResultadoOperacion<DetalleRetiroDto>> ObtenerDetalleAsync(
        string tokenSesion, int retiroId)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<DetalleRetiroDto>.Error("Sesión inválida o expirada.");

        var detalle = await _repo.ObtenerDetalleAsync(uid.Value, retiroId);
        return detalle == null
            ? ResultadoOperacion<DetalleRetiroDto>.Error("Retiro no encontrado.")
            : ResultadoOperacion<DetalleRetiroDto>.Ok(detalle);
    }

    public async Task<ResultadoOperacion<DetalleRetiroDto>> RetirarAsync(
        string tokenSesion, RetirarDto dto)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<DetalleRetiroDto>.Error("Sesión inválida o expirada.");

        try
        {
            var resultado = await _repo.RegistrarRetiroAsync(uid.Value, dto);
            return ResultadoOperacion<DetalleRetiroDto>.Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return ResultadoOperacion<DetalleRetiroDto>.Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacion<DetalleRetiroDto>.Error(ex.Message);
        }
    }

    private async Task<int?> ResolverUsuarioIdAsync(string tokenSesion)
    {
        if (string.IsNullOrWhiteSpace(tokenSesion)) return null;
        var sesion = await _sesionRepo.ObtenerSesionActivaAsync(tokenSesion);
        if (sesion == null) return null;
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(sesion.UsuarioId);
        return usuario?.Estado == "Activo" ? usuario.UsuarioId : null;
    }
}
