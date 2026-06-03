using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace X_Chang.API.Helpers
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Obtiene el id del usuario autenticado.
        ///
        /// La autenticación real (login + JWT) corresponde a las historias US-001/US-002,
        /// que está implementando otro integrante del equipo. Para no bloquear el avance del
        /// backend, este helper:
        ///   1) Lee primero el claim "UsuarioId" del token JWT (cuando ya esté integrado).
        ///   2) Si no hay token, acepta la cabecera "X-Usuario-Id" para poder probar los
        ///      endpoints (p. ej. desde Swagger o Postman) mientras tanto.
        ///
        /// Cuando el login esté listo, basta con habilitar [Authorize] en los controladores
        /// y quitar el fallback de la cabecera.
        /// </summary>
        public static int? GetUsuarioId(this ControllerBase controller)
        {
            // 1) Claim del JWT (formato del repo del profesor: claim "UsuarioId").
            var claim = controller.User?.FindFirst("UsuarioId")?.Value;
            if (int.TryParse(claim, out var idDesdeClaim))
                return idDesdeClaim;

            // 2) Fallback de pruebas: cabecera X-Usuario-Id.
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
