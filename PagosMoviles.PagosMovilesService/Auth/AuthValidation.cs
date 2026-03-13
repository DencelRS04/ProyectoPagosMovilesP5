using System.Net.Http.Headers;

namespace PagosMoviles.PagosMovilesService.Services;

public class AuthValidationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;

    public AuthValidationService(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _cfg = cfg;
    }

    public async Task<bool> ValidateAsync(string bearerToken)
    {
        //Modo demo: si ponen "DEV" en Swagger Authorize, deja pasar.
        if (string.Equals(bearerToken, "DEV", StringComparison.OrdinalIgnoreCase))
            return true;

        var baseUrl = _cfg["Services:AuthSrv"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return false;


        try
        {
            _http.BaseAddress = new Uri(baseUrl);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            //
            var resp = await _http.GetAsync("/");
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}