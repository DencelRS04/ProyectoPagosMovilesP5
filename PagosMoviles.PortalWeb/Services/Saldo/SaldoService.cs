using Microsoft.AspNetCore.Http;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Saldo;
using PagosMoviles.Shared.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
                .PostAsJsonAsync("gateway/admin/core/accounts/balance", request);

            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponseDto<SaldoResponseDto>>();

            return wrapper?.Datos ?? new SaldoResponseDto();
        }
    }
}