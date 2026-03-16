using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Saldo;
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
            var client = _factory.CreateClient("gateway");
            var token = _ctx.HttpContext?.Session.GetString("jwt_token");

            // TEMPORAL — quitar cuando el login esté integrado
            if (string.IsNullOrEmpty(token))
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AcHJ1ZWJhLmNvbSIsIlVzdWFyaW9JZCI6IjIiLCJleHAiOjE3NzM2MTI2OTR9.ETohFLNLNJwL3PFAMNgWXFwS3HnEkIMM91XmxF33Je0";

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
                .ReadFromJsonAsync<ApiResponse<SaldoResponseDto>>();
            return wrapper?.Datos ?? new SaldoResponseDto();
        }
    }
}