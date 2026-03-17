using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Saldo;
using PagosMoviles.Shared.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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

        public async Task<SaldoResponseDto> ConsultarSaldo(string telefono, string identificacion)
        {
            var request = new SaldoRequestDto
            {
                Telefono = telefono,
                Identificacion = identificacion
            };

            var response = await ClienteAutenticado().PostAsJsonAsync("accounts/balance", request);
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseDto<SaldoResponseDto>>();
            return wrapper?.Datos ?? new SaldoResponseDto();
        }
    }
}