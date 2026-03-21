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
            try
            {
                var client = _httpClientFactory.CreateClient("GatewayApi");
                Console.WriteLine($"[DEBUG SERVICE] BaseAddress: '{client.BaseAddress}'");
                Console.WriteLine($"[DEBUG SERVICE] Auth header: '{client.DefaultRequestHeaders.Authorization}'");

                var httpResponse = await client.GetAsync("gateway/admin/entidad");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new List<EntidadViewModel>();
                }

                var response = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<List<EntidadViewModel>>>();

                return response?.Datos ?? new List<EntidadViewModel>();
            }
            catch
            {
                return new List<EntidadViewModel>();
            }
        }

        public async Task<(bool ok, string mensaje)> CrearAsync(EntidadCreateModel entidad)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GatewayApi");
                var model = new
                {
                    codigoEntidad = entidad.CodigoEntidad,
                    nombreInstitucion = entidad.NombreInstitucion
                };

                var httpResponse = await client.PostAsJsonAsync("gateway/admin/entidad", model);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var error = await httpResponse.Content
                        .ReadFromJsonAsync<ApiResponse<object>>();

                    return (false, error?.Descripcion ?? "Error al crear entidad");
                }

                return (true, "Entidad creada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear entidad: {ex.Message}");
            }
        }

        public async Task<EntidadViewModel?> ObtenerPorIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GatewayApi");
                var httpResponse = await client.GetAsync($"gateway/admin/entidad/{id}");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var response = await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<EntidadViewModel>>();

                return response?.Datos;
            }
            catch
            {
                return null;
            }
        }

        public async Task ActualizarAsync(int entidadId, EntidadEditModel entidad)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");
            var model = new
            {
                codigoEntidad = entidad.CodigoEntidad,
                nombreInstitucion = entidad.NombreInstitucion
            };

            var httpResponse = await client.PutAsJsonAsync(
                $"gateway/admin/entidad/{entidadId}",
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
            try
            {
                var client = _httpClientFactory.CreateClient("GatewayApi");
                var httpResponse = await client.DeleteAsync($"gateway/admin/entidad/{id}");

                ApiResponse<object>? response = null;

                try
                {
                    response = await httpResponse.Content
                        .ReadFromJsonAsync<ApiResponse<object>>();
                }
                catch
                {
                    response = null;
                }

                if (!httpResponse.IsSuccessStatusCode)
                    return (false, response?.Descripcion ?? "No se pudo eliminar la entidad");

                return (true, response?.Descripcion ?? "Entidad eliminada correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"No se pudo eliminar la entidad: {ex.Message}");
            }
        }

        private class ApiResponse<T>
        {
            public int Codigo { get; set; }
            public string? Descripcion { get; set; }
            public T? Datos { get; set; }
        }
    }
}