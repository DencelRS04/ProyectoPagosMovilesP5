using Microsoft.AspNetCore.Http;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Pantallas;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PagosMoviles.AdminWeb.Services.Pantallas
{
    public class PantallasService : IPantallasService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public PantallasService(IHttpClientFactory factory, IHttpContextAccessor ctx)
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

        public async Task<List<PantallaDto>> ObtenerPantallas()
        {
            var response = await ClienteAutenticado().GetAsync("gateway/admin/pantalla");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponseDto<List<PantallaDto>>>(json, _jsonOptions);

            return wrapper?.Datos ?? new List<PantallaDto>();
        }

        public async Task<PantallaDto?> ObtenerPantalla(int id)
        {
            var response = await ClienteAutenticado().GetAsync($"gateway/admin/pantalla/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponseDto<PantallaDto>>(json, _jsonOptions);

            return wrapper?.Datos;
        }

        public async Task CrearPantalla(PantallaCreateDto dto)
        {
            var client = ClienteAutenticado();
            var response = await client.PostAsJsonAsync("gateway/admin/pantalla", dto);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Status: {response.StatusCode} - {content}");
        }

        public async Task ActualizarPantalla(int id, PantallaCreateDto dto)
        {
            var response = await ClienteAutenticado().PutAsJsonAsync($"gateway/admin/pantalla/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarPantalla(int id)
        {
            var response = await ClienteAutenticado().DeleteAsync($"gateway/admin/pantalla/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}