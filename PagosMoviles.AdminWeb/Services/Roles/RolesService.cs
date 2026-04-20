using Microsoft.AspNetCore.Http;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Roles;
using PagosMoviles.Shared.Models;
using System.Net;
using System.Net.Http.Headers;

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

            var usuario = SessionHelper.ObtenerUsuarioSesion(_ctx.HttpContext!.Session);
            var token = usuario?.AccessToken;

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<List<RolDto>> ObtenerRoles()
        {
            var response = await ClienteAutenticado().GetAsync("gateway/admin/rol");
            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponseDto<List<RolDto>>>();
            return wrapper?.Datos ?? new List<RolDto>();
        }

        public async Task<RolDto?> ObtenerRol(int id)
        {
            var response = await ClienteAutenticado().GetAsync($"gateway/admin/rol/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<ApiResponseDto<RolDto>>();
            return wrapper?.Datos;
        }

        public async Task CrearRol(RolCreateDto dto)
        {
            var response = await ClienteAutenticado().PostAsJsonAsync("gateway/admin/rol", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarRol(int id, RolCreateDto dto)
        {
            var response = await ClienteAutenticado().PutAsJsonAsync($"gateway/admin/rol/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarRol(int id)
        {
            var response = await ClienteAutenticado().DeleteAsync($"gateway/admin/rol/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}