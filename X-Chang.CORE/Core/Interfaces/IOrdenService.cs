using System;
using System.Collections.Generic;
using System.Text;
using X_Chang.CORE.DTOs;

namespace X_Chang.CORE.Interfaces;

public interface IOrdenService
{
    Task<LibroOrdenesDto> ObtenerLibroOrdenesAsync(int parMonedaId);
    Task<LibroOrdenesDetalleDto> ObtenerLibroOrdenesDetalleAsync(int parMonedaId, int limite = 10);
}
