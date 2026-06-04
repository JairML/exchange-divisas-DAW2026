namespace X_Chang.CORE.Core.DTOs.Common;

public record PagedResultDto<T>(List<T> Items, int Total, int Pagina, int TamanoPagina);
