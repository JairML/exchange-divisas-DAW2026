namespace X_Chang.CORE.Core.DTOs
{
    // Moneda para poblar las listas desplegables (registro, depósito, retiro, etc.).
    public class MonedaDTO
    {
        public int MonedaId { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}
