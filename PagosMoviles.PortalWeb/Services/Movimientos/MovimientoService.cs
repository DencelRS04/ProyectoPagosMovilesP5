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

        var response = await client.GetFromJsonAsync<ApiResponse<List<MovimientosViewModels>>>(
            $"movimientos/{telefono}"
        );

        return response.Data;
    }
}