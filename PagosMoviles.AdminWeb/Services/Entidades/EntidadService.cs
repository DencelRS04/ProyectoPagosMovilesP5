using System.Net.Http.Json;
using PagosMoviles.AdminWeb.Models.Entidades;

namespace PagosMoviles.AdminWeb.Services.Entidades
{
    public class EntidadService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EntidadService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<EntidadViewModel>> ListarAsync()
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var response = await client.GetFromJsonAsync<ApiResponse<List<EntidadViewModel>>>("gateway/admin/entidad");

            return response?.Datos ?? new List<EntidadViewModel>();
        }

        public async Task<(bool ok, string mensaje)> CrearAsync(EntidadCreateModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.PostAsJsonAsync("gateway/admin/entidad", entidad);

            var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();

            return (httpResponse.IsSuccessStatusCode, result?.Descripcion ?? "");
        }

        // 🔥 AQUÍ ESTABA EL ERROR
        public async Task<(bool ok, string mensaje)> ActualizarAsync(int id, EntidadEditModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.PutAsJsonAsync($"gateway/admin/entidad/{id}", entidad);

            var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();

            return (httpResponse.IsSuccessStatusCode, result?.Descripcion ?? "");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.DeleteAsync($"gateway/admin/entidad/{id}");

            var result = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();

            return (httpResponse.IsSuccessStatusCode, result?.Descripcion ?? "");
        }
    }
}