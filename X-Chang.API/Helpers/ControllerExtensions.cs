using Microsoft.AspNetCore.Mvc;

namespace X_Chang.API.Helpers;

public static class ControllerExtensions
{
    public static int? GetUsuarioId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst("UsuarioId");
        if (claim == null || !int.TryParse(claim.Value, out var id))
            return null;
        return id;
    }
}
