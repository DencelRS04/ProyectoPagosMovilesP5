using System.Text.Json.Serialization;

namespace PagosMoviles.PortalWeb.Models
{
    public class ApiResponse<T>
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // ✅ Mapear "datos" del JSON a "Data" en C#
        [JsonPropertyName("datos")]
        public T? Data { get; set; }
    }
}