using System.Net.Http.Headers;

namespace PagosMoviles.UsuariosService.Services;

public class AuthClient
{
    private readonly HttpClient _http;

    public AuthClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> ValidateAsync(string bearerToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "auth/validate");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var resp = await _http.SendAsync(req);
        return resp.IsSuccessStatusCode;
    }
}