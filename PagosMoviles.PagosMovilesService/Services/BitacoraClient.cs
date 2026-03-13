using System.Net.Http.Json;

namespace PagosMoviles.PagosMovilesService.Services;

public class BitacoraClient
{
    private readonly HttpClient _http;

    public BitacoraClient(HttpClient http)
    {
        _http = http;
    }

    public async Task RegistrarAsync(string usuario, string accion, object? anterior = null, object? actual = null)
    {
        try
        {
            var payload = new
            {
                Usuario = usuario,
                Accion = accion,
                Anterior = anterior,
                Actual = actual
            };

            await _http.PostAsJsonAsync("bitacora", payload);
        }
        catch (Exception ex)
        {
            // Log local si falla, pero no bloquea la operación
            System.Diagnostics.Debug.WriteLine($"Error registrando bitácora: {ex.Message}");
        }
    }
}