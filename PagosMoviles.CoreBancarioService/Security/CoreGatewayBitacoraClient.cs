using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.CoreBancarioService.Services
{
    public sealed class CoreGatewayBitacoraClient
    {
        private readonly IHttpClientFactory _factory;

        public CoreGatewayBitacoraClient(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task RegistrarAsync(string usuario, string accion, object? anterior = null, object? actual = null, string? bearerToken = null)
        {
            try
            {
                var client = _factory.CreateClient("ApiGateway");

                var payload = new
                {
                    usuario,
                    accion,
                    anterior,
                    actual
                };

                using var req = new HttpRequestMessage(HttpMethod.Post, "bitacora");
                req.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                if (!string.IsNullOrWhiteSpace(bearerToken))
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                await client.SendAsync(req);
            }
            catch
            {
                // No romper flujo de negocio si falla bitácora
            }
        }
    }
}