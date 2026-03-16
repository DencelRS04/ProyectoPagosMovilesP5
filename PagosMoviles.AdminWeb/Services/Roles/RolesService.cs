using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Roles;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
            var token = _ctx.HttpContext?.Session.GetString("jwt_token");

            // TOKEN TEMPORAL DE PRUEBAS, ESTO SE QUITA CUANDO SE INTEGRE EL LOGIN
            if (string.IsNullOrEmpty(token))
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AcHJ1ZWJhLmNvbSIsIlVzdWFyaW9JZCI6IjIiLCJleHAiOjE3NzM2MTI2OTR9.ETohFLNLNJwL3PFAMNgWXFwS3HnEkIMM91XmxF33Je0";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<List<RolDto>> ObtenerRoles()
        {
            var response = await ClienteAutenticado().GetAsync("rol");
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<RolDto>>>();
            return wrapper?.Datos ?? new List<RolDto>();
        }

        public async Task<RolDto?> ObtenerRol(int id)
        {
            var response = await ClienteAutenticado().GetAsync($"rol/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponse<RolDto>>();
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