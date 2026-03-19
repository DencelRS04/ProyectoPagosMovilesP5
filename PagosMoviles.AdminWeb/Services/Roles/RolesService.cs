using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Roles;
using PagosMoviles.Shared.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PagosMoviles.AdminWeb.Services.Roles
{
    public class RolesService : IRolesService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public RolesService(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {
            _factory = factory;
            _ctx = ctx;
        }

        private HttpClient ClienteAutenticado()
        {
            var client = _factory.CreateClient("gateway");

            var json = _ctx.HttpContext?.Session.GetString("USUARIO_SESION");
            string token = null;

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var usuario = JsonSerializer.Deserialize<UsuarioSesionModel>(json);
                    token = usuario?.AccessToken;
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public async Task<List<RolDto>> ObtenerRoles()
        {
            var response = await ClienteAutenticado().GetAsync("rol");
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseDto<List<RolDto>>>();
            return wrapper?.Datos ?? new List<RolDto>();
        }

        public async Task<RolDto?> ObtenerRol(int id)
        {
            var response = await ClienteAutenticado().GetAsync($"rol/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseDto<RolDto>>();
            return wrapper?.Datos;
        }

        public async Task CrearRol(RolCreateDto dto)
        {
            var response = await ClienteAutenticado().PostAsJsonAsync("rol", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarRol(int id, RolCreateDto dto)
        {
            var response = await ClienteAutenticado().PutAsJsonAsync($"rol/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarRol(int id)
        {
            var response = await ClienteAutenticado().DeleteAsync($"rol/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}