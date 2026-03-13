using System.Net.Http.Json;
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

            var response = await client.GetFromJsonAsync<ApiResponse<List<ClienteViewModel>>>("gateway/admin/core/client");

            return response?.Datos ?? new List<ClienteViewModel>();
        }

        public async Task<ClienteViewModel?> ObtenerPorIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var response = await client.GetFromJsonAsync<ApiResponse<ClienteViewModel>>($"gateway/admin/core/client/{id}");

            return response?.Datos;
        }

        public async Task<(bool ok, string mensaje)> CrearAsync(ClienteViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.PostAsJsonAsync("gateway/admin/core/client", model);

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<ClienteViewModel>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo crear el cliente");

            return (true, response?.Descripcion ?? "Cliente creado correctamente");
        }

        public async Task<(bool ok, string mensaje)> ActualizarAsync(int id, ClienteViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.PutAsJsonAsync($"gateway/admin/core/client/{id}", model);

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo actualizar el cliente");

            return (true, response?.Descripcion ?? "Cliente actualizado correctamente");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.DeleteAsync($"gateway/admin/core/client/{id}");

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo eliminar el cliente");

            return (true, response?.Descripcion ?? "Cliente desactivado correctamente");
        }
    }
}