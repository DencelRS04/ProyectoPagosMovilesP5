using System.Text.Json.Serialization;

namespace PagosMoviles.AdminWeb.Models.Transacciones
{
    public class TransaccionViewModel
    {
        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        [JsonPropertyName("telefonoOrigen")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [JsonPropertyName("telefonoDestino")]
        public string TelefonoDestino { get; set; } = string.Empty;

        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }
    }
}