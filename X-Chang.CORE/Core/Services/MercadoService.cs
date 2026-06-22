using X_Chang.CORE.Core.DTOs.Mercado;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class MercadoService : IMercadoService
    {
        private readonly IMercadoRepository _repository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public MercadoService(IMercadoRepository repository, ISesionUsuarioRepository sesionRepository)
        {
            _repository = repository;
            _sesionRepository = sesionRepository;
        }

        public async Task<OperacionesActivasResponseDto> ObtenerOperacionesActivasAsync(string tokenSesion, FiltroOperacionesActivasDto filtro)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: false);
            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            ValidarRegistrosPorPagina(filtro.RegistrosPorPagina, new[] { "10", "20", "50", "100" });
            if (filtro.Pagina < 1) filtro.Pagina = 1;
            return await _repository.ObtenerOperacionesActivasAsync(usuarioId, filtro);
        }

        public Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId, bool verTodasOrdenes, bool verTodasOfertas)
        {
            if (parMonedaId <= 0)
                throw new ArgumentException("El par de monedas no existe.");

            return _repository.ObtenerLibroOrdenesAsync(parMonedaId, verTodasOrdenes, verTodasOfertas);
        }

        public async Task<ResumenOrdenCompraDto> ObtenerResumenOrdenCompraAsync(string tokenSesion, CrearOrdenCompraRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOrden(request);
            return await _repository.ObtenerResumenOrdenCompraAsync(usuarioId, request);
        }

        public async Task<OrdenCompraResultadoDto> CrearOrdenCompraAsync(string tokenSesion, CrearOrdenCompraRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOrden(request);
            return await _repository.CrearOrdenCompraAsync(usuarioId, request);
        }

        public async Task<ResumenOfertaVentaDto> ObtenerResumenOfertaVentaAsync(string tokenSesion, CrearOfertaVentaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOferta(request);
            return await _repository.ObtenerResumenOfertaVentaAsync(usuarioId, request);
        }

        public async Task<OfertaVentaResultadoDto> CrearOfertaVentaAsync(string tokenSesion, CrearOfertaVentaRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: true);
            ValidarOferta(request);
            return await _repository.CrearOfertaVentaAsync(usuarioId, request);
        }

        public async Task<PanelAdministrativoDto> ObtenerPanelAdministrativoAsync(string tokenSesion, FiltroPanelAdministrativoDto filtro)
        {
            var usuarioId = await ObtenerUsuarioIdAsync(tokenSesion, bloquearRestringido: false);
            var esAdmin = await _repository.EsAdministradorActivoAsync(usuarioId);
            if (!esAdmin)
                throw new UnauthorizedAccessException("Acceso no autorizado");

            ValidarFechas(filtro.FechaDesde, filtro.FechaHasta);
            return await _repository.ObtenerPanelAdministrativoAsync(filtro);
        }

        private async Task<int> ObtenerUsuarioIdAsync(string tokenSesion, bool bloquearRestringido)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);
            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            if (bloquearRestringido && sesion.Usuario.Estado == "Restringido")
                throw new InvalidOperationException("Su cuenta se encuentra restringida y no puede generar órdenes, ofertas ni operaciones inmediatas.");

            return sesion.UsuarioId;
        }

        private static void ValidarOrden(CrearOrdenCompraRequestDto request)
        {
            if (request.ParMonedaId <= 0)
                throw new ArgumentException("El par de monedas no existe.");
            if (request.CantidadAObtener <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");
            if (request.PrecioUnitario <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0");
        }

        private static void ValidarOferta(CrearOfertaVentaRequestDto request)
        {
            if (request.ParMonedaId <= 0)
                throw new ArgumentException("El par de monedas no existe.");
            if (request.CantidadAVender <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");
            if (request.PrecioUnitario <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0");
        }

        private static void ValidarFechas(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha final debe ser posterior a la fecha inicial");
        }

        private static void ValidarRegistrosPorPagina(string valor, string[] permitidos)
        {
            if (!permitidos.Contains(valor))
                throw new ArgumentException("Cantidad de registros por página inválida.");
        }
    }
}
