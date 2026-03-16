using System.Net.Http.Json;
using PagosMoviles.PortalWeb.Models.Afiliacion;

namespace PagosMoviles.PortalWeb.Services.Afiliacion
{
    public class AfiliacionService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AfiliacionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ClienteExisteAsync(string identificacion)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var response = await client.GetFromJsonAsync<ApiResponse<ClienteExisteResponse>>(
                $"gateway/admin/core/client-exists?identificacion={Uri.EscapeDataString(identificacion)}");

            return response?.Datos?.Existe ?? false;
        }

        public async Task<(bool ok, string mensaje)> RegistrarAsync(AfiliacionViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var payload = new
            {
                identificacion = model.Identificacion,
                numeroCuenta = model.NumeroCuenta,
                telefono = model.Telefono
            };

            var httpResponse = await client.PostAsJsonAsync("gateway/auth/register", payload);
            var rawContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(rawContent))
                    return (false, $"Error HTTP {(int)httpResponse.StatusCode}: respuesta vacía");

                try
                {
                    var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(
                        rawContent,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    return (false, errorResponse?.Descripcion ?? $"Error HTTP {(int)httpResponse.StatusCode}");
                }
                catch
                {
                    return (false, rawContent);
                }
            }

            if (string.IsNullOrWhiteSpace(rawContent))
                return (true, "Afiliación registrada correctamente");

            try
            {
                var okResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(
                    rawContent,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return (true, okResponse?.Descripcion ?? "Afiliación registrada correctamente");
            }
            catch
            {
                return (true, "Afiliación registrada, pero la respuesta no vino en JSON");
            }
        }

        private class ClienteExisteResponse
        {
            public bool Existe { get; set; }
        }
    }
}