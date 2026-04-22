using System.Net.Http.Json;
using System.Text.Json;
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

            var response = await client.GetAsync(
                $"gateway/admin/transactions/por-fecha?fecha={fecha:yyyy-MM-dd}");

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)response.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return new List<TransaccionViewModel>();

            var resultado = JsonSerializer.Deserialize<ApiResponse<List<TransaccionViewModel>>>(
                raw,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return resultado?.Datos ?? new List<TransaccionViewModel>();
        }
    }
}