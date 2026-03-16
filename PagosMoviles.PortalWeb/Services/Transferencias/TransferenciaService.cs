using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.DTOs;
using PagosMoviles.Shared.DTOs.Transferencias;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PagosMoviles.PortalWeb.Services.Transferencias
{
    public class TransferenciaService : ITransferenciaService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;

        public TransferenciaService(IHttpClientFactory factory, IHttpContextAccessor ctx)
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
                token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AcHJ1ZWJhLmNvbSIsIlVzdWFyaW9JZCI6IjIiLCJleHAiOjE3NzM2OTM4Mzd9.SlZDXyfVxQEnED54ZjQqsHVxlS2SoG1k7VMfCLn6RHM";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<TransferenciaResponseDto> RealizarTransferencia(TransferenciaRequestDto dto)
        {
            var response = await ClienteAutenticado()
                .PostAsJsonAsync("api/Transactions/route", dto);
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content
                .ReadFromJsonAsync<ApiResponse<TransferenciaResponseDto>>();
            return wrapper?.Datos ?? new TransferenciaResponseDto
            {
                Codigo = 200,
                Descripcion = "Transacción aplicada"
            };
        }
    }
}
