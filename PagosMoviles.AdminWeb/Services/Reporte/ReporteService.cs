using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Models.Transacciones; // Aquí debería estar TransaccionViewModel
using PagosMoviles.AdminWeb.Pages.Reportes;

public class ReporteService
{
    private readonly IHttpClientFactory _http;

    public ReporteService(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<List<TransaccionViewModel>> ObtenerPorFecha(DateTime fecha)
    //                    ↑ Usando el ViewModel correcto
    {
        var client = _http.CreateClient("GatewayApi");

        var response = await client.GetFromJsonAsync<ApiResponse<List<TransaccionViewModel>>>(
            $"transacciones?fecha={fecha:yyyy-MM-dd}"
        );

        return response?.Data ?? new List<TransaccionViewModel>();
    }
}