using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.UsuariosService.Services;

public class BitacoraClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public BitacoraClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public async Task RegistrarAsync(string usuario, string accion, object? anterior, object? actual, string? bearerToken)
    {
        try
        {
            var baseUrl = _cfg["Services:PagosMovilesApi"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                return; // no rompe si no está configurado

            var url = baseUrl.Trim().TrimEnd('/') + "/bitacora";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);

            if (!string.IsNullOrWhiteSpace(bearerToken))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            req.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    usuario,
                    accion,
                    anterior,
                    actual
                }, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
                Encoding.UTF8,
                "application/json"
            );

            await _http.SendAsync(req);
        }
        catch
        {
            // No hacemos throw: bitácora nunca debe tumbar el flujo
        }
    }
}