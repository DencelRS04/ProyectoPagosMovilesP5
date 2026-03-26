using Microsoft.AspNetCore.Http;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Models.Transferencias;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
            var client = _factory.CreateClient("GatewayApi");

            var usuario = SessionHelper.ObtenerUsuarioSesion(_ctx.HttpContext!.Session);

            if (usuario != null && !string.IsNullOrWhiteSpace(usuario.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", usuario.AccessToken);
            }

            return client;
        }

        public async Task<TransferenciaResponseDto> RealizarTransferencia(TransferenciaRequestDto dto)
        {
            var client = ClienteAutenticado();

            Console.WriteLine("JSON enviado:");
            Console.WriteLine(JsonSerializer.Serialize(dto));

            var response = await client.PostAsJsonAsync(
                "gateway/admin/transactions/route",
                dto);

            var raw = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)response.StatusCode}");
            Console.WriteLine($"Respuesta: {raw}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(raw, null, response.StatusCode);

            var result = JsonSerializer.Deserialize<TransferenciaResponseDto>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result ?? new TransferenciaResponseDto
            {
                Codigo = (int)response.StatusCode,
                Descripcion = "Operación completada"
            };
        }
    }
}