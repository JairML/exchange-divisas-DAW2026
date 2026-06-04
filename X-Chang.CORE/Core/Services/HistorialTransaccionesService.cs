using X_Chang.CORE.Core.DTOs.HistorialTransacciones;
using X_Chang.CORE.Core.Interfaces;

namespace X_Chang.CORE.Core.Services
{
    public class HistorialTransaccionesService : IHistorialTransaccionesService
    {
        private readonly IHistorialTransaccionesRepository _repository;
        private readonly ISesionUsuarioRepository _sesionRepository;

        public HistorialTransaccionesService(
            IHistorialTransaccionesRepository repository,
            ISesionUsuarioRepository sesionRepository)
        {
            _repository = repository;
            _sesionRepository = sesionRepository;
        }

        public async Task<HistorialTransaccionesResponseDto> ObtenerHistorialAsync(
            string tokenSesion, HistorialTransaccionesRequestDto request)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (request.FechaDesde.HasValue && request.FechaHasta.HasValue
                && request.FechaDesde.Value > request.FechaHasta.Value)
                throw new ArgumentException("La fecha final debe ser posterior a la fecha inicial.");

            if (request.NumeroPagina < 1)
                request.NumeroPagina = 1;

            var response = new HistorialTransaccionesResponseDto();

            bool cargarTodo = string.IsNullOrWhiteSpace(request.Columna);

            if (cargarTodo || request.Columna == "OrdenesCompra")
                response.OrdenesCompra = await _repository.ObtenerOrdenesCompraAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "OfertasVenta")
                response.OfertasVenta = await _repository.ObtenerOfertasVentaAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "ComprasInmediatas")
                response.ComprasInmediatas = await _repository.ObtenerComprasInmediatasAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "VentasInmediatas")
                response.VentasInmediatas = await _repository.ObtenerVentasInmediatasAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "Depositos")
                response.Depositos = await _repository.ObtenerDepositosAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            if (cargarTodo || request.Columna == "Retiros")
                response.Retiros = await _repository.ObtenerRetirosAsync(
                    usuarioId, request.FechaDesde, request.FechaHasta,
                    request.NumeroPagina, request.RegistrosPorPagina);

            return response;
        }

        public async Task<HistorialTransaccionesResponseDto> ObtenerParaExportarAsync(
            string tokenSesion, DateTime? fechaDesde, DateTime? fechaHasta, string? columna)
        {
            var usuarioId = await ObtenerUsuarioIdDesdeSesionAsync(tokenSesion);

            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value > fechaHasta.Value)
                throw new ArgumentException("La fecha final debe ser posterior a la fecha inicial.");

            var request = new HistorialTransaccionesRequestDto
            {
                Columna = columna,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                NumeroPagina = 1,
                RegistrosPorPagina = 0
            };

            // Reutilizar ObtenerHistorialAsync con token ya validado
            var response = new HistorialTransaccionesResponseDto();

            bool cargarTodo = string.IsNullOrWhiteSpace(columna);

            if (cargarTodo || columna == "OrdenesCompra")
                response.OrdenesCompra = await _repository.ObtenerOrdenesCompraAsync(
                    usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "OfertasVenta")
                response.OfertasVenta = await _repository.ObtenerOfertasVentaAsync(
                    usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "ComprasInmediatas")
                response.ComprasInmediatas = await _repository.ObtenerComprasInmediatasAsync(
                    usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "VentasInmediatas")
                response.VentasInmediatas = await _repository.ObtenerVentasInmediatasAsync(
                    usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "Depositos")
                response.Depositos = await _repository.ObtenerDepositosAsync(
                    usuarioId, fechaDesde, fechaHasta, 1, 0);

            if (cargarTodo || columna == "Retiros")
                response.Retiros = await _repository.ObtenerRetirosAsync(
                    usuarioId, fechaDesde, fechaHasta, 1, 0);

            return response;
        }

        private async Task<int> ObtenerUsuarioIdDesdeSesionAsync(string tokenSesion)
        {
            if (string.IsNullOrWhiteSpace(tokenSesion))
                throw new UnauthorizedAccessException("Sesión no enviada.");

            var sesion = await _sesionRepository.ObtenerSesionActivaAsync(tokenSesion);

            if (sesion == null)
                throw new UnauthorizedAccessException("Sesión inválida o expirada.");

            return sesion.UsuarioId;
        }
    }
}
