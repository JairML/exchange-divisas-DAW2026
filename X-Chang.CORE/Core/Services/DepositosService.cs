using X_Chang.CORE.Core.DTOs;
using X_Chang.CORE.Core.DTOs.Billetera;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services;

public class DepositosService : IDepositosService
{
    private readonly IDepositosRepository _repo;
    private readonly IDepositoService _depositoService;
    private readonly ISesionUsuarioRepository _sesionRepo;
    private readonly IUsuarioRepository _usuarioRepo;

    public DepositosService(
        IDepositosRepository repo,
        IDepositoService depositoService,
        ISesionUsuarioRepository sesionRepo,
        IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _depositoService = depositoService;
        _sesionRepo = sesionRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<ResultadoOperacion<(List<DetalleDepositoDto> items, int total)>> ListarAsync(
        string tokenSesion, int pagina, int tamano)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<(List<DetalleDepositoDto>, int)>.Error("Sesión inválida o expirada.");

        var items = await _repo.ListarAsync(uid.Value, pagina, tamano);
        var total = await _repo.ContarAsync(uid.Value);
        return ResultadoOperacion<(List<DetalleDepositoDto>, int)>.Ok((items, total));
    }

    public async Task<ResultadoOperacion<DetalleDepositoDto>> ObtenerDetalleAsync(
        string tokenSesion, int depositoId)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<DetalleDepositoDto>.Error("Sesión inválida o expirada.");

        var detalle = await _repo.ObtenerDetalleAsync(uid.Value, depositoId);
        return detalle == null
            ? ResultadoOperacion<DetalleDepositoDto>.Error("Depósito no encontrado.")
            : ResultadoOperacion<DetalleDepositoDto>.Ok(detalle);
    }

    public async Task<ResultadoOperacion<DetalleDepositoDto>> DepositarAsync(
        string tokenSesion, int monedaId, int metodoPagoId, decimal monto)
    {
        var uid = await ResolverUsuarioIdAsync(tokenSesion);
        if (uid == null)
            return ResultadoOperacion<DetalleDepositoDto>.Error("Sesión inválida o expirada.");

        // Delegar en el servicio existente que ya maneja la transacción completa
        var resultado = await _depositoService.RegistrarDeposito(
            uid.Value,
            new Core.DTOs.DepositoCreateDTO { MonedaId = monedaId, MetodoPagoId = metodoPagoId, Monto = monto });

        if (!resultado.Exito)
            return ResultadoOperacion<DetalleDepositoDto>.Error(resultado.Mensaje!);

        // Leer el detalle recién creado para devolver el DTO canónico
        var detalle = await _repo.ObtenerDetalleAsync(uid.Value, resultado.Data!.DepositoId);
        return ResultadoOperacion<DetalleDepositoDto>.Ok(detalle!);
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
