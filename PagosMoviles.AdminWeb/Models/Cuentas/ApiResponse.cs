using System.Text.Json.Serialization;

namespace PagosMoviles.AdminWeb.Models.Cuentas
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("exito")]
        public bool Exito { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("datos")]
        public T Datos { get; set; }
    }
}