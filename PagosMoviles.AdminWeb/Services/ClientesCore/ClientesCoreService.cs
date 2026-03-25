using System.Net.Http.Json;
using System.Text.Json;
using PagosMoviles.AdminWeb.Models.ClientesCore;

namespace PagosMoviles.AdminWeb.Services.ClientesCore
{
    public class ClientesCoreService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientesCoreService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ClienteViewModel>> ListarAsync()
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.GetAsync("gateway/admin/core/client");
            var raw = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return new List<ClienteViewModel>();

            var response = JsonSerializer.Deserialize<ApiResponse<List<ClienteViewModel>>>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return response?.Datos ?? new List<ClienteViewModel>();
        }

        public async Task<ClienteViewModel?> ObtenerPorIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.GetAsync($"gateway/admin/core/client/{id}");
            var raw = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var response = JsonSerializer.Deserialize<ApiResponse<ClienteViewModel>>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return response?.Datos;
        }

        public async Task<(bool ok, string mensaje)> CrearAsync(ClienteViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            if (model.FechaNacimiento == null)
                return (false, "Debe seleccionar una fecha de nacimiento.");

            var dto = new
            {
                identificacion = model.Identificacion?.Trim(),
                tipoIdentificacion = model.TipoIdentificacion?.Trim(),
                nombreCompleto = model.NombreCompleto?.Trim(),
                fechaNacimiento = model.FechaNacimiento.Value,
                telefono = model.Telefono?.Trim(),
                activo = model.Activo
            };

            var httpResponse = await client.PostAsJsonAsync("gateway/admin/core/client", dto);
            var raw = await httpResponse.Content.ReadAsStringAsync();

            Console.WriteLine($"POST Crear Cliente Status: {(int)httpResponse.StatusCode}");
            Console.WriteLine($"POST Crear Cliente Respuesta: {raw}");

            if (!httpResponse.IsSuccessStatusCode)
                return (false, $"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return (true, "Cliente creado correctamente");

            var response = JsonSerializer.Deserialize<ApiResponse<ClienteViewModel>>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return (true, response?.Descripcion ?? "Cliente creado correctamente");
        }

        public async Task<(bool ok, string mensaje)> ActualizarAsync(int id, ClienteViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            if (model.FechaNacimiento == null)
                return (false, "Debe seleccionar una fecha de nacimiento.");

            var dto = new
            {
                clienteId = id,
                identificacion = model.Identificacion?.Trim(),
                tipoIdentificacion = model.TipoIdentificacion?.Trim(),
                nombreCompleto = model.NombreCompleto?.Trim(),
                fechaNacimiento = model.FechaNacimiento.Value,
                telefono = model.Telefono?.Trim(),
                activo = model.Activo
            };

            var httpResponse = await client.PutAsJsonAsync($"gateway/admin/core/client/{id}", dto);
            var raw = await httpResponse.Content.ReadAsStringAsync();

            Console.WriteLine($"PUT Actualizar Cliente Status: {(int)httpResponse.StatusCode}");
            Console.WriteLine($"PUT Actualizar Cliente Respuesta: {raw}");

            if (!httpResponse.IsSuccessStatusCode)
                return (false, $"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return (true, "Cliente actualizado correctamente");

            var response = JsonSerializer.Deserialize<ApiResponse<object>>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return (true, response?.Descripcion ?? "Cliente actualizado correctamente");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.DeleteAsync($"gateway/admin/core/client/{id}");
            var raw = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, $"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return (true, "Cliente desactivado correctamente");

            var response = JsonSerializer.Deserialize<ApiResponse<object>>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return (true, response?.Descripcion ?? "Cliente desactivado correctamente");
        }
    }
}