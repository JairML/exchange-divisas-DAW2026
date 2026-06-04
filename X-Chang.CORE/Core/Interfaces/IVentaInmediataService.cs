using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.Core.DTOs.VentaInmediata;

namespace X_Chang.CORE.Core.Interfaces
{
    public interface IVentaInmediataService
    {
        Task<ResumenVentaInmediataDto> ObtenerResumenVentaNormalAsync(
            string tokenSesion,
            VentaInmediataRequestDto request);

        Task<VentaInmediataResponseDto> ConfirmarVentaNormalAsync(
            string tokenSesion,
            ConfirmarVentaInmediataRequestDto request);

        Task<TiempoEstimadoBusquedaRutaVentaDto>
            ObtenerTiempoEstimadoBusquedaRutaAsync(
                int cantidadMaximaSaltos);

        Task<ResultadoBusquedaRutaVentaDto> BuscarMejorRutaAsync(
            string tokenSesion,
            BuscarMejorRutaVentaRequestDto request);

        Task<bool> CancelarBusquedaRutaAsync(
            string tokenSesion,
            int busquedaRutaId);

        Task<VentaInmediataResponseDto> ConfirmarVentaPorRutaAsync(
            string tokenSesion,
            ConfirmarVentaRutaRequestDto request);
    }
}