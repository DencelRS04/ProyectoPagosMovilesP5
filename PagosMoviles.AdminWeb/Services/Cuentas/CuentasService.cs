using System.Net.Http.Json;
using System.Text.Json;
using PagosMoviles.AdminWeb.Models.Cuentas;

namespace PagosMoviles.AdminWeb.Services.Cuentas
{
    public class CuentasService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CuentasService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<CuentaViewModel>> ListarAsync()
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.GetAsync("gateway/admin/core/accounts");
            var raw = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return new List<CuentaViewModel>();

            var response = JsonSerializer.Deserialize<ApiResponse<List<CuentaViewModel>>>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return response?.Datos ?? new List<CuentaViewModel>();
        }

        public async Task<List<CuentaViewModel>> ListarPorClienteAsync(int clienteId)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.GetAsync($"gateway/admin/core/accounts/cliente/{clienteId}");
            var raw = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            if (string.IsNullOrWhiteSpace(raw))
                return new List<CuentaViewModel>();

            var response = JsonSerializer.Deserialize<ApiResponse<List<CuentaViewModel>>>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return response?.Datos ?? new List<CuentaViewModel>();
        }

        public async Task<(bool ok, string mensaje)> CrearAsync(CuentaCreateModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            // Autogenerar número de cuenta si viene vacío
            if (string.IsNullOrWhiteSpace(model.NumeroCuenta))
                model.NumeroCuenta = DateTime.UtcNow.ToString("yyyyMMdd") +
                                     Random.Shared.Next(1000, 9999);

            var dto = new
            {
                clienteId = model.ClienteId,
                numeroCuenta = model.NumeroCuenta.Trim(),
                tipoCuenta = model.TipoCuenta?.Trim(),
                saldo = model.Saldo
            };

            var httpResponse = await client.PostAsJsonAsync("gateway/admin/core/accounts", dto);
            var raw = await httpResponse.Content.ReadAsStringAsync();

            Console.WriteLine($"POST Crear Cuenta Status: {(int)httpResponse.StatusCode}");
            Console.WriteLine($"POST Crear Cuenta Respuesta: {raw}");

            if (!httpResponse.IsSuccessStatusCode)
                return (false, $"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            var response = JsonSerializer.Deserialize<ApiResponse<object>>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (true, response?.Descripcion ?? "Cuenta creada correctamente");
        }

        public async Task<(bool ok, string mensaje)> ActualizarAsync(int id, CuentaEditModel model)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var dto = new
            {
                numeroCuenta = model.NumeroCuenta?.Trim(),
                tipoCuenta = model.TipoCuenta?.Trim(),
                saldo = model.Saldo
            };

            var httpResponse = await client.PutAsJsonAsync($"gateway/admin/core/accounts/{id}", dto);
            var raw = await httpResponse.Content.ReadAsStringAsync();

            Console.WriteLine($"PUT Actualizar Cuenta Status: {(int)httpResponse.StatusCode}");
            Console.WriteLine($"PUT Actualizar Cuenta Respuesta: {raw}");

            if (!httpResponse.IsSuccessStatusCode)
                return (false, $"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            var response = JsonSerializer.Deserialize<ApiResponse<object>>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (true, response?.Descripcion ?? "Cuenta actualizada correctamente");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("GatewayApi");

            var httpResponse = await client.DeleteAsync($"gateway/admin/core/accounts/{id}");
            var raw = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                return (false, $"Error HTTP {(int)httpResponse.StatusCode}: {raw}");

            var response = JsonSerializer.Deserialize<ApiResponse<object>>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (true, response?.Descripcion ?? "Cuenta eliminada correctamente");
        }
    }
}