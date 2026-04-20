using Microsoft.AspNetCore.Http;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Saldo;
using System.Net.Http.Headers;

namespace PagosMoviles.PortalWeb.Services.Saldo
{
    public class SaldoService : ISaldoService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public SaldoService(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {
            _factory = factory;
            _ctx = ctx;
        }

        private HttpClient ClienteAutenticado()
        {
            var client = _factory.CreateClient("GatewayApi");

            var usuario = SessionHelper.ObtenerUsuarioSesion(_ctx.HttpContext!.Session);

            if (usuario != null && !string.IsNullOrWhiteSpace(usuario.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", usuario.AccessToken);
            }

            return client;
        }

        public async Task<SaldoResponseDto> ConsultarSaldo(string telefono, string identificacion)
        {
            var request = new SaldoRequestDto
            {
                Telefono = telefono,
                Identificacion = identificacion
            };

            var response = await ClienteAutenticado()
                .PostAsJsonAsync("gateway/trans/accounts/balance", request);

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)response.StatusCode}: {raw}");

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiResponseDto<SaldoResponseDto>>(
                raw,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return wrapper?.Datos ?? new SaldoResponseDto();
        }
    }
}