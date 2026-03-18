using System.Text.Json.Serialization;

namespace PagosMoviles.AdminWeb.Models.Entidades
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("exito")]
        public bool Exito { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("datos")]  // ← mapea "datos" del JSON a "Datos" de C#
        public T Datos { get; set; }
    }
}