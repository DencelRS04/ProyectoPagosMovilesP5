using System.Net.Http.Json;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Models.Transacciones;

namespace PagosMoviles.AdminWeb.Services.Reporte
{
    public class ReporteService
    {
        private readonly IHttpClientFactory _http;

        public ReporteService(IHttpClientFactory http)
        {
            _http = http;
        }

        public async Task<List<TransaccionViewModel>> ObtenerPorFecha(DateTime fecha)
        {
            var client = _http.CreateClient("GatewayApi");
            var response = await client.GetFromJsonAsync<ApiResponse<List<TransaccionViewModel>>>(
                $"gateway/admin/transactions/por-fecha?fecha={fecha:yyyy-MM-dd}"
            );
            return response?.Datos ?? new List<TransaccionViewModel>();
        }
    }
}