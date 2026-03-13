using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.PagosMovilesService.Services
{
    public class CoreSrvClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public CoreSrvClient(HttpClient http)
        {
            _http = http;
        }

        // ===== Respuesta estándar del Core =====
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = "";
            public T? Data { get; set; }
        }

        // ===== Request del Core =====
        public class ConsultaCuentaDto
        {
            public string Identificacion { get; set; } = "";
            public string NumeroCuenta { get; set; } = "";
        }

        // ===== Movimiento del Core =====
        public class MovimientoCoreDto
        {
            public string TipoMovimiento { get; set; } = "";
            public decimal Monto { get; set; }
            public DateTime Fecha { get; set; }
        }

        private HttpRequestMessage BuildPost(string path, object body, string? bearerToken)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, path);

            if (!string.IsNullOrWhiteSpace(bearerToken))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            req.Content = new StringContent(
                JsonSerializer.Serialize(body, _json),
                Encoding.UTF8,
                "application/json"
            );

            return req;
        }

        private static ApiResponse<T> Fail<T>(string msg) =>
            new() { Success = false, Message = msg, Data = default };

        // =========================================================
        // SRV16: POST /core/movimientos/ultimos
        // =========================================================
        public async Task<ApiResponse<List<MovimientoCoreDto>>?> UltimosMovimientosAsync(
            string identificacion,
            string numeroCuenta,
            string? bearerToken = null)
        {
            var dto = new ConsultaCuentaDto
            {
                Identificacion = identificacion,
                NumeroCuenta = numeroCuenta
            };

            using var req = BuildPost("/core/movimientos/ultimos", dto, bearerToken);
            using var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            try
            {
                var r = JsonSerializer.Deserialize<ApiResponse<List<MovimientoCoreDto>>>(body, _json);
                return r ?? Fail<List<MovimientoCoreDto>>($"Respuesta no válida del Core. HTTP={(int)resp.StatusCode}");
            }
            catch
            {
                return Fail<List<MovimientoCoreDto>>($"Respuesta no válida del Core. HTTP={(int)resp.StatusCode}");
            }
        }
    }
}