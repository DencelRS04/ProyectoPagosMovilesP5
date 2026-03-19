using System.Text.Json.Serialization;

namespace PagosMoviles.Shared.DTOs.Transferencias
{
    public class TransferenciaRequestDto
    {
        [JsonPropertyName("entidadOrigen")]
        public string EntidadOrigen { get; set; } = string.Empty;

        [JsonPropertyName("entidadDestino")]
        public string EntidadDestino { get; set; } = string.Empty;

        [JsonPropertyName("telefonoOrigen")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [JsonPropertyName("nombreOrigen")]
        public string NombreOrigen { get; set; } = string.Empty;

        [JsonPropertyName("telefonoDestino")]
        public string TelefonoDestino { get; set; } = string.Empty;

        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }

    public class TransferenciaResponseDto
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}