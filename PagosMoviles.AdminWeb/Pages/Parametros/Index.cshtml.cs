using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Parametros;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.AdminWeb.Pages.Parametros
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpFactory;

        public IndexModel(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public List<ParametroViewModel> Parametros { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty]
        public ParametroFormModel Formulario { get; set; } = new();

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }

        private HttpClient CrearClienteConToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _httpFactory.CreateClient("ParametroApi");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = CrearClienteConToken();
            var response = await client.GetAsync("parametro");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ApiRespuesta<List<ParametroViewModel>>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Parametros = resultado?.Datos ?? new();

                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    var b = Busqueda.ToUpper();
                    Parametros = Parametros
                        .Where(p => p.ParametroId.Contains(b))
                        .ToList();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            bool esNuevo = string.IsNullOrEmpty(Formulario.ParametroIdOriginal);

            // Al editar, el ID no se puede cambiar — omitir su validación
            if (!esNuevo)
                ModelState.Remove("Formulario.ParametroId");

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var client = CrearClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            HttpResponseMessage response;

            if (esNuevo)
            {
                var dto = new { parametroId = Formulario.ParametroId, valor = Formulario.Valor };
                var content = new StringContent(JsonSerializer.Serialize(dto, opciones),
                    Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                response = await client.PostAsync("parametro", content);
            }
            else
            {
                var dto = new { valor = Formulario.Valor };
                var content = new StringContent(JsonSerializer.Serialize(dto, opciones),
                    Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                response = await client.PutAsync($"parametro/{Formulario.ParametroIdOriginal}", content);
            }

            if (response.IsSuccessStatusCode)
            {
                MensajeExito = "Parámetro guardado correctamente.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                MensajeError = $"Error al guardar el parámetro. ({(int)response.StatusCode}): {errorBody}";
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(string id)
        {
            var client = CrearClienteConToken();
            var response = await client.DeleteAsync($"parametro/{id}");

            if (response.IsSuccessStatusCode)
                MensajeExito = "Parámetro eliminado correctamente.";
            else
                MensajeError = "No se pudo eliminar el parámetro.";

            await OnGetAsync();
            return Page();
        }

        private class ApiRespuesta<T>
        {
            public int Codigo { get; set; }
            public string? Descripcion { get; set; }
            public T? Datos { get; set; }
        }
    }
}