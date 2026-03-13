using System.Net;
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
            var resp = await _http.GetAsync($"/core/client-exists?identificacion={identificacion}");

            if (!resp.IsSuccessStatusCode)
                return SrvResponse<bool>.Fail("Error consultando Core");

            var existe = await resp.Content.ReadFromJsonAsync<bool>();

            return existe
                ? SrvResponse<bool>.Ok(true, "Cliente existe en el core")
                : SrvResponse<bool>.Fail("Cliente no encontrado en el core");
        }

        private class CoreExistsResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public bool data { get; set; }
        }
    }
}