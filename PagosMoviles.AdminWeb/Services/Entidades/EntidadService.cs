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

            var response = await client.GetFromJsonAsync<ApiResponse<List<EntidadViewModel>>>
                ("gateway/admin/entidad");

            return response?.Datos ?? new List<EntidadViewModel>();
        }

        public async Task<EntidadViewModel?> ObtenerPorIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var response = await client.GetFromJsonAsync<ApiResponse<EntidadViewModel>>
                ($"gateway/admin/entidad/{id}");

            return response?.Datos;
        }

        public async Task<(bool ok, string mensaje)> CrearAsync(EntidadViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.PostAsJsonAsync
                ("gateway/admin/entidad", model);

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<EntidadViewModel>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo crear la entidad");

            return (true, response?.Descripcion ?? "Entidad creada correctamente");
        }

        public async Task<(bool ok, string mensaje)> ActualizarAsync(int id, EntidadViewModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.PutAsJsonAsync
                ($"gateway/admin/entidad/{id}", model);

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<object>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo actualizar la entidad");

            return (true, response?.Descripcion ?? "Entidad actualizada correctamente");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.DeleteAsync
                ($"gateway/admin/entidad/{id}");

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<object>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo eliminar la entidad");

            return (true, response?.Descripcion ?? "Entidad eliminada correctamente");
        }

        internal async Task CrearAsync(EntidadCreateModel entidad)
        {
            throw new NotImplementedException();
        }

        internal async Task ActualizarAsync(int identificador, EntidadEditModel entidad)
        {
            throw new NotImplementedException();
        }
    }
}