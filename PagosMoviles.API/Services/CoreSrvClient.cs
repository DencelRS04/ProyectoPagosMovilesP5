using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.API.Services;

public class CoreSrvClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public CoreSrvClient(HttpClient http)
    {
        _http = http;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
    }

    public class ConsultaCuentaDto
    {
        public string Identificacion { get; set; } = "";
        public string NumeroCuenta { get; set; } = "";
    }

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

    // SRV16 Core: POST /core/movimientos/ultimos
    // ✅ Agregar esta clase para mapear la respuesta real del Core
    public class CoreApiResponse<T>
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = "";
        public T? Datos { get; set; }
    }

    // SRV16 Core: POST /core/movimientos/ultimos
    public async Task<ApiResponse<List<MovimientoCoreDto>>?> UltimosMovimientosAsync(
        string identificacion,
        string numeroCuenta,
        string? bearerToken = null)
    {
        var identificacionLimpia = identificacion.Replace("-", "").Trim();

        var dto = new ConsultaCuentaDto
        {
            Identificacion = identificacionLimpia,
            NumeroCuenta = numeroCuenta
        };

        using var req = BuildPost("/core/movimientos/ultimos", dto, bearerToken);
        using var resp = await _http.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            return Fail<List<MovimientoCoreDto>>(
                $"Core Bancario respondió HTTP {(int)resp.StatusCode}: {body}"
            );
        }

        try
        {
            // ✅ Deserializar con el formato real del Core
            var coreResp = JsonSerializer.Deserialize<CoreApiResponse<List<MovimientoCoreDto>>>(body, _json);

            if (coreResp == null)
                return Fail<List<MovimientoCoreDto>>("Respuesta no válida del Core (JSON nulo).");

            // ✅ Mapear al formato ApiResponse que usa el controlador
            return new ApiResponse<List<MovimientoCoreDto>>
            {
                Success = coreResp.Codigo == 200,
                Message = coreResp.Descripcion,
                Data = coreResp.Datos
            };
        }
        catch
        {
            return Fail<List<MovimientoCoreDto>>("Respuesta no válida del Core (JSON inválido).");
        }
    }
}