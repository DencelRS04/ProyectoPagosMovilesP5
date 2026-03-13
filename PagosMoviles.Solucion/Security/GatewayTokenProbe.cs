using System.Net.Http.Headers;

namespace PagosMoviles.UsuariosService.Security;

public sealed class GatewayTokenProbe
{
    private readonly IHttpClientFactory _factory;

    public GatewayTokenProbe(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var client = _factory.CreateClient("GatewayApi");

        // OJO: esta ruta debe existir en PagosMoviles.API
        // Si tu API tiene /auth/validate, perfecto.
        // Si tiene otra, me decís y lo cambiamos aquí.
        using var req = new HttpRequestMessage(HttpMethod.Get, "auth/validate");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await client.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }
}