using System.Net.Http.Headers;

namespace PagosMoviles.CoreBancarioService.Security
{
    public sealed class CoreGatewayTokenProbe
    {
        private readonly IHttpClientFactory _factory;

        public CoreGatewayTokenProbe(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var client = _factory.CreateClient("ApiGateway");

                using var req = new HttpRequestMessage(HttpMethod.Get, "auth/validate");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var resp = await client.SendAsync(req);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}