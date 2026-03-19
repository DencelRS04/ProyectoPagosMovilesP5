using System.Net.Http.Json;
using PagosMoviles.PortalWeb.Models;
using PagosMoviles.PortalWeb.Models.Movimientos;

public class MovimientoService
{
    private readonly IHttpClientFactory _http;

    public MovimientoService(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<List<MovimientosViewModels>> ObtenerUltimosMovimientos(string telefono)
    {
        var client = _http.CreateClient("GatewayApi");

        if (client.BaseAddress == null)
            throw new InvalidOperationException("HttpClient 'GatewayApi' no tiene BaseAddress configurada.");

        // ✅ GET con solo teléfono
        var httpResponse = await client.GetAsync(
            $"gateway/admin/accounts/transactions/{Uri.EscapeDataString(telefono)}"
        );

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("401 Unauthorized: token ausente o expirado.");

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            return new List<MovimientosViewModels>();

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content
      .ReadFromJsonAsync<ApiResponse<MovimientosResponse>>();

        // ✅ Log temporal
        Console.WriteLine($"[DEBUG] apiResponse null: {apiResponse == null}");
        Console.WriteLine($"[DEBUG] Data null: {apiResponse?.Data == null}");
        Console.WriteLine($"[DEBUG] Movimientos null: {apiResponse?.Data?.Movimientos == null}");
        Console.WriteLine($"[DEBUG] Movimientos count: {apiResponse?.Data?.Movimientos?.Count ?? -1}");

        return apiResponse?.Data?.Movimientos ?? new List<MovimientosViewModels>();

    }
}