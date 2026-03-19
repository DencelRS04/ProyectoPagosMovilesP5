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
            var response = await client.GetFromJsonAsync<ApiResponse<List<EntidadViewModel>>>(
                "gateway/admin/entidad"
            );
            return response?.Datos ?? new List<EntidadViewModel>();
        }



        public async Task<(bool ok, string mensaje)> CrearAsync(EntidadCreateModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");
            var model = new
            {
                codigoEntidad = entidad.CodigoEntidad,
                nombreInstitucion = entidad.NombreInstitucion
            };
            var response = await client.PostAsJsonAsync("gateway/admin/entidad", model);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content
                    .ReadFromJsonAsync<ApiResponse<object>>();
                return (false, error?.Descripcion ?? "Error al crear entidad");
            }

            return (true, "Entidad creada correctamente");
        }

        // ✅ Solo un ActualizarAsync, sin duplicado
        // ✅ Nuevo método para obtener por CodigoEntidad
        // ✅ Corregir URL de ObtenerPorIdAsync (estaba apuntando a /core/ por error)
        public async Task<EntidadViewModel?> ObtenerPorIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");
            var response = await client.GetFromJsonAsync<ApiResponse<EntidadViewModel>>(
                $"gateway/admin/entidad/{id}"  // ✅ era /core/{id}, ahora /entidad/{id}
            );
            return response?.Datos;
        }

        // ✅ ActualizarAsync recibe int en lugar de string
        public async Task ActualizarAsync(int entidadId, EntidadEditModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");
            var model = new
            {
                codigoEntidad = entidad.CodigoEntidad,
                nombreInstitucion = entidad.NombreInstitucion
            };
            var httpResponse = await client.PutAsJsonAsync(
                $"gateway/admin/entidad/{entidadId}",  // ✅ int en la URL
                model
            );
            if (!httpResponse.IsSuccessStatusCode)
            {
                var response = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<EntidadViewModel>>();
                throw new Exception(response?.Descripcion ?? "Error al actualizar entidad");
            }
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");
            var httpResponse = await client.DeleteAsync($"gateway/admin/entidad/{id}");
            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<object>>();
            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo eliminar la entidad");
            return (true, response?.Descripcion ?? "Entidad eliminada correctamente");
        }
    }
}