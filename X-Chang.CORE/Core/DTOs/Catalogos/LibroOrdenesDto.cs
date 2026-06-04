namespace X_Chang.CORE.Core.DTOs.Catalogos;

public class LibroOrdenesDto
{
    public List<NivelLibroDto> Compras { get; set; } = new();
    public List<NivelLibroDto> Ventas { get; set; } = new();
}
