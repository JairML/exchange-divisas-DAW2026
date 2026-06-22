using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace X_Chang.API.Helpers
{
    public static class ControllerExtensions
    {
        public static int? GetUsuarioId(this ControllerBase controller)
        {
            var claim = controller.User?.FindFirst("UsuarioId")?.Value;
            if (int.TryParse(claim, out var idDesdeClaim))
                return idDesdeClaim;

            if (controller.Request.Headers.TryGetValue("X-Usuario-Id", out var header))
            {
                var valor = header.FirstOrDefault();
                if (int.TryParse(valor, out var idDesdeHeader))
                    return idDesdeHeader;
            }

            return null;
        }
    }
}
