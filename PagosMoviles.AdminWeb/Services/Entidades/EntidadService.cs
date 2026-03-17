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
        public async Task<EntidadViewModel?> ObtenerPorIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var response = await client.GetFromJsonAsync<ApiResponse<EntidadViewModel>>
                ($"gateway/admin/core/{id}");

            return response?.Datos;
        }

        public async Task CrearAsync(EntidadCreateModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var model = new
            {
                codigoEntidad = entidad.CodigoEntidad,
                nombreInstitucion = entidad.NombreInstitucion
            };

            var response = await client.PostAsJsonAsync(
                "gateway/admin/entidad",
                model
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al crear: {error}");
            }
        }

        public async Task ActualizarAsync(string codigoEntidad, EntidadEditModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var model = new EntidadViewModel
            {
                CodigoEntidad = entidad.CodigoEntidad,
                NombreInstitucion = entidad.NombreInstitucion
            };

            var httpResponse = await client.PutAsJsonAsync(
                $"gateway/admin/entidad/{codigoEntidad}",
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

            var httpResponse = await client.DeleteAsync
                ($"gateway/admin/entidad/{id}");

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<object>>();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, response?.Descripcion ?? "No se pudo eliminar la entidad");

            return (true, response?.Descripcion ?? "Entidad eliminada correctamente");
        }

        internal async Task ActualizarAsync(int identificador, EntidadEditModel entidad)
        {
            throw new NotImplementedException();
        }

       

       
    }
}