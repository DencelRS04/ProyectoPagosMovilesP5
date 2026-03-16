using System.Net.Http.Json;
using System.Text.Json;
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

            var httpResponse = await client.GetAsync(
                $"gateway/admin/core/client-exists?identificacion={Uri.EscapeDataString(identificacion)}");

            var rawContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode || string.IsNullOrWhiteSpace(rawContent))
                return false;

            try
            {
                var response = JsonSerializer.Deserialize<ApiResponse<ClienteExisteResponse>>(
                    rawContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return response?.Datos?.Existe ?? false;
            }
            catch
            {
                return false;
            }
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
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
                        rawContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    return (false, errorResponse?.Descripcion ?? rawContent);
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
                var okResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
                    rawContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return (true, okResponse?.Descripcion ?? "Afiliación registrada correctamente");
            }
            catch
            {
                return (true, "Afiliación registrada correctamente");
            }
        }

        private class ClienteExisteResponse
        {
            public bool Existe { get; set; }
        }
    }
}