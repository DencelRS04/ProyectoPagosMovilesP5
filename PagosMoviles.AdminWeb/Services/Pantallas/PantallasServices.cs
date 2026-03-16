using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Pantallas;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PagosMoviles.AdminWeb.Services.Pantallas
{
    public class PantallasService : IPantallasService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public PantallasService(IHttpClientFactory factory, IHttpContextAccessor ctx)
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

        public async Task<List<PantallaDto>> ObtenerPantallas()
        {
            var response = await ClienteAutenticado().GetAsync("pantalla");
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<PantallaDto>>>();
            return wrapper?.Datos ?? new List<PantallaDto>();
        }

        public async Task<PantallaDto?> ObtenerPantalla(int id)
        {
            var response = await ClienteAutenticado().GetAsync($"pantalla/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponse<PantallaDto>>();
            return wrapper?.Datos;
        }

        public async Task CrearPantalla(PantallaCreateDto dto)
        {
            var response = await ClienteAutenticado().PostAsJsonAsync("pantalla", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarPantalla(int id, PantallaCreateDto dto)
        {
            var response = await ClienteAutenticado().PutAsJsonAsync($"pantalla/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarPantalla(int id)
        {
            var response = await ClienteAutenticado().DeleteAsync($"pantalla/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}