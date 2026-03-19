using System;

namespace PagosMoviles.Shared.Models
{
    public class ApiResponse<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T Data { get; set; }

        public static ApiResponse<T> Ok(T data, string mensaje = "")
        {
            return new ApiResponse<T>
            {
                Exito = true,
                Mensaje = mensaje,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string mensaje)
        {
            return new ApiResponse<T>
            {
                Exito = false,
                Mensaje = mensaje,
                Data = default(T)
            };
        }
    }
}