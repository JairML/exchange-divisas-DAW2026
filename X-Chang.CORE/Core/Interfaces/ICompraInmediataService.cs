using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.CompraInmediata;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface ICompraInmediataService
    {
        Task<ResumenCompraInmediataDto> ObtenerResumenCompraNormalAsync(
            string tokenSesion,
            CompraInmediataRequestDto request);

        Task<CompraInmediataResponseDto> ConfirmarCompraNormalAsync(
            string tokenSesion,
            ConfirmarCompraInmediataRequestDto request);

        Task<TiempoEstimadoBusquedaRutaDto> ObtenerTiempoEstimadoBusquedaRutaAsync(
            int cantidadMaximaSaltos);

        Task<ResultadoBusquedaRutaCompraDto> BuscarMejorRutaAsync(
            string tokenSesion,
            BuscarMejorRutaCompraRequestDto request);

        Task<bool> CancelarBusquedaRutaAsync(
            string tokenSesion,
            int busquedaRutaId);

        Task<CompraInmediataResponseDto> ConfirmarCompraPorRutaAsync(
            string tokenSesion,
            ConfirmarCompraRutaRequestDto request);
    }
}