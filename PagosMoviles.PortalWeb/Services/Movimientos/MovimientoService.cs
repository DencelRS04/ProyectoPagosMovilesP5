using System.Net.Http.Json;
using PagosMoviles.PortalWeb.Models;
using PagosMoviles.PortalWeb.Models.Movimientos;
using System.Web;

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

        // DEBUG: verificar BaseAddress en logs si es necesario
        // Console.WriteLine($"Gateway BaseAddress: {client.BaseAddress}");

        if (client.BaseAddress == null)
            throw new InvalidOperationException("HttpClient 'GatewayApi' no tiene BaseAddress configurada.");

        var safeTelefono = Uri.EscapeDataString(telefono ?? string.Empty);
        var relativePath = $"gateway/admin/movimiento/ultimos?telefono={safeTelefono}";

        // Usar GetAsync para capturar códigos de estado
        var httpResponse = await client.GetAsync(relativePath);

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Manejo: redirigir a login, lanzar excepción o devolver lista vacía según tu flujo
            throw new UnauthorizedAccessException("401 Unauthorized: token ausente o expirado.");
        }

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<MovimientosViewModels>();
        }

        httpResponse.EnsureSuccessStatusCode();

        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<List<MovimientosViewModels>>>();
        return apiResponse?.Data ?? new List<MovimientosViewModels>();
    }
}