using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Catalogos;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class MetodosPagoService : IMetodosPagoService
{
    private readonly IMetodosPagoRepository _repo;
    private readonly ISesionUsuarioRepository _sesionRepo;
    private readonly IUsuarioRepository _usuarioRepo;

    public MetodosPagoService(
        IMetodosPagoRepository repo,
        ISesionUsuarioRepository sesionRepo,
        IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _sesionRepo = sesionRepo;
        _usuarioRepo = usuarioRepo;
    }

    public Task<List<MetodoPagoDto>> ObtenerTodosAsync() =>
        _repo.ObtenerTodosActivosAsync();

    public Task<MetodoPagoDto?> ObtenerPorIdAsync(int metodoPagoId) =>
        _repo.ObtenerPorIdAsync(metodoPagoId);

    public async Task<ResultadoOperacion<List<MetodoPagoDto>>> ObtenerParaDepositoAsync(string tokenSesion)
    {
        var paisResult = await ResolverPaisIdAsync(tokenSesion);
        if (!paisResult.Exito)
            return ResultadoOperacion<List<MetodoPagoDto>>.Error(paisResult.Mensaje!);

        var metodos = await _repo.ObtenerParaPaisAsync(paisResult.Data!.Value, ["Pago", "Ambos"]);
        return ResultadoOperacion<List<MetodoPagoDto>>.Ok(metodos);
    }

    public async Task<ResultadoOperacion<List<MetodoPagoDto>>> ObtenerParaRetiroAsync(string tokenSesion)
    {
        var paisResult = await ResolverPaisIdAsync(tokenSesion);
        if (!paisResult.Exito)
            return ResultadoOperacion<List<MetodoPagoDto>>.Error(paisResult.Mensaje!);

        var metodos = await _repo.ObtenerParaPaisAsync(paisResult.Data!.Value, ["Cobro", "Ambos"]);
        return ResultadoOperacion<List<MetodoPagoDto>>.Ok(metodos);
    }

    private async Task<ResultadoOperacion<int?>> ResolverPaisIdAsync(string tokenSesion)
    {
        if (string.IsNullOrWhiteSpace(tokenSesion))
            return ResultadoOperacion<int?>.Error("No se envió el token de sesión.");

        var sesion = await _sesionRepo.ObtenerSesionActivaAsync(tokenSesion);
        if (sesion == null)
            return ResultadoOperacion<int?>.Error("Sesión inválida o expirada.");

        var usuario = await _usuarioRepo.ObtenerPorIdAsync(sesion.UsuarioId);
        if (usuario == null)
            return ResultadoOperacion<int?>.Error("Usuario no encontrado.");

        return ResultadoOperacion<int?>.Ok(usuario.PaisId);
    }
}
