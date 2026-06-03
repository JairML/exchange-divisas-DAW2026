namespace X_Chang.CORE.Core.DTOs
{
    /// <summary>
    /// Envoltura simple para devolver el resultado de una operación de negocio
    /// junto con un mensaje de validación legible para el usuario.
    /// Permite que los servicios devuelvan los mensajes exactos exigidos por
    /// los criterios de aceptación (p. ej. "Saldo insuficiente") y que el
    /// controlador decida el código HTTP correspondiente.
    /// </summary>
    public class ResultadoOperacion<T>
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public T? Data { get; set; }

        public static ResultadoOperacion<T> Ok(T data) =>
            new() { Exito = true, Data = data };

        public static ResultadoOperacion<T> Error(string mensaje) =>
            new() { Exito = false, Mensaje = mensaje };
    }
}
