namespace X_Chang.CORE.Core.DTOs.Precios
{
    public class FiltroParesMonedaDto
    {
        // ISO de la moneda entregada, o "Cualquiera"
        public string MonedaEntrega { get; set; } = "Cualquiera";

        // ISO de la moneda obtenida, o "Cualquiera"
        public string MonedaObtiene { get; set; } = "Cualquiera";

        // "FechaReciente" | "Volumen" | "MayorPrecioCompra" | "MenorPrecioVenta" | "Margen"
        public string Criterio { get; set; } = "MayorPrecioCompra";

        // "asc" | "desc"
        public string Direccion { get; set; } = "desc";

        // Agrupa PEN/USD y USD/PEN en un único registro canónico (ISO menor/ISO mayor)
        public bool ColapsarParesInversos { get; set; } = false;

        public int Pagina { get; set; } = 1;

        // "10" | "20" | "40" | "100" | "200" | "400" | "Todos"
        public string RegistrosPorPagina { get; set; } = "20";
    }
}
