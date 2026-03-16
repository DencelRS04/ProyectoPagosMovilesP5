using System.Net.Http.Json;
using PagosMoviles.UsuariosService.Utils;

namespace PagosMoviles.UsuariosService.Services
{
    public class CoreClientService
    {
        private readonly HttpClient _http;

        public CoreClientService(HttpClient http)
        {
            _http = http;
        }

        public async Task<SrvResponse<bool>> VerificarClienteEnCoreAsync(string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
                return SrvResponse<bool>.Fail("La identificación es requerida");

            var resp = await _http.GetAsync($"core/client-exists?identificacion={Uri.EscapeDataString(identificacion)}");

            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync();
                return SrvResponse<bool>.Fail(
                    $"Error consultando Core. HTTP {(int)resp.StatusCode}. {errorBody}");
            }

            var content = await resp.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return SrvResponse<bool>.Fail("El Core respondió vacío");

            CoreApiResponse<CoreExistsData>? result;

            try
            {
                result = System.Text.Json.JsonSerializer.Deserialize<CoreApiResponse<CoreExistsData>>(
                    content,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                return SrvResponse<bool>.Fail($"No se pudo interpretar la respuesta del Core: {ex.Message}");
            }

            if (result?.Datos == null)
                return SrvResponse<bool>.Fail("Respuesta inválida del Core");

            return result.Datos.Existe
                ? SrvResponse<bool>.Ok(true, "Cliente existe en el core")
                : SrvResponse<bool>.Fail("Cliente no encontrado en el core");
        }

        private class CoreApiResponse<T>
        {
            public int Codigo { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public T? Datos { get; set; }
        }

        private class CoreExistsData
        {
            public bool Existe { get; set; }
        }
    }
}