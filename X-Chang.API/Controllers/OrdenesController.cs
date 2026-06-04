using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using X_Chang.CORE.Core.Interfaces;
using X_Chang.CORE.DTOs;

namespace X_Chang.API.Controllers;

[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;

    public OrdenesController(IOrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest request)
    {
        try
        {
            var result = await _ordenService.CrearOrdenCompraAsync(UsuarioId, request);
            return CreatedAtAction(nameof(ObtenerOrden), new { id = result.OrdenCompraId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private object ObtenerOrden()
    {
        throw new NotImplementedException();
    }
}