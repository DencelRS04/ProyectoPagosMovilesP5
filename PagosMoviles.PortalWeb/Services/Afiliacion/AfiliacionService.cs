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
                $"gateway/admin/core/client/exists?identificacion={Uri.EscapeDataString(identificacion)}");

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

            var httpResponse = await client.PostAsJsonAsync(
                "gateway/admin/core/afiliacion/register",
                payload);

            var rawContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(rawContent))
                    return (false, $"Error HTTP {(int)httpResponse.StatusCode}: respuesta vacía");

                try
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(
                        rawContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (apiResponse != null && !string.IsNullOrWhiteSpace(apiResponse.Descripcion))
                        return (false, apiResponse.Descripcion);
                }
                catch
                {
                }

                try
                {
                    var validation = JsonSerializer.Deserialize<ValidationErrorResponse>(
                        rawContent,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (validation != null)
                    {
                        var mensajes = new List<string>();

                        if (!string.IsNullOrWhiteSpace(validation.Title))
                            mensajes.Add(validation.Title);

                        if (validation.Errors != null)
                        {
                            foreach (var error in validation.Errors)
                            {
                                if (error.Value != null)
                                {
                                    foreach (var detalle in error.Value)
                                    {
                                        if (!string.IsNullOrWhiteSpace(detalle))
                                            mensajes.Add(detalle);
                                    }
                                }
                            }
                        }

                        if (mensajes.Count > 0)
                            return (false, string.Join(" ", mensajes));
                    }
                }
                catch
                {
                }

                return (false, rawContent);
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

        private class ValidationErrorResponse
        {
            public string? Title { get; set; }
            public int Status { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }
    }
}