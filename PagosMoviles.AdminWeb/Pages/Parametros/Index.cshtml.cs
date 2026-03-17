using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.AdminWeb.Models.Parametros;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        public bool ReobrirModal { get; set; }

        private HttpClient CrearClienteConToken()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);
            var token = usuario?.AccessToken;
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
            else
            {
                MensajeError = $"Error al cargar parámetros. ({(int)response.StatusCode})";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            bool esNuevo = string.IsNullOrEmpty(Formulario.ParametroIdOriginal);

            if (esNuevo && !string.IsNullOrEmpty(Formulario.ParametroId))
            {
                Formulario.ParametroId = Formulario.ParametroId.ToUpperInvariant();

                // Limpiar el resultado del binding anterior y revalidar manualmente
                ModelState.Remove("Formulario.ParametroId");
                if (string.IsNullOrWhiteSpace(Formulario.ParametroId))
                    ModelState.AddModelError("Formulario.ParametroId", "El identificador es obligatorio.");
                else if (!Regex.IsMatch(Formulario.ParametroId, @"^[A-Z]{1,10}$"))
                    ModelState.AddModelError("Formulario.ParametroId", "Solo letras mayúsculas, entre 1 y 10 caracteres.");
            }

            if (!esNuevo)
                ModelState.Remove("Formulario.ParametroId");

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                MensajeError = "Datos inválidos: " + string.Join(" | ", errores);
                ReobrirModal = true;
                await CargarListaAsync();
                return Page();
            }

            var client = CrearClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            HttpResponseMessage response;

            try
            {
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
            }
            catch (Exception ex)
            {
                MensajeError = $"No se pudo conectar con el servidor: {ex.Message}";
                ReobrirModal = true;
                await CargarListaAsync();
                return Page();
            }

            if (response.IsSuccessStatusCode)
            {
                MensajeExito = esNuevo
                    ? "Parámetro creado correctamente."
                    : "Parámetro actualizado correctamente.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                MensajeError = $"Error al guardar el parámetro ({(int)response.StatusCode}): {errorBody}";
                ReobrirModal = true;
            }

            await CargarListaAsync();
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

            await CargarListaAsync();
            return Page();
        }

        private async Task CargarListaAsync()
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
                    Parametros = Parametros.Where(p => p.ParametroId.Contains(b)).ToList();
                }
            }
        }

        private class ApiRespuesta<T>
        {
            public int Codigo { get; set; }
            public string? Descripcion { get; set; }
            public T? Datos { get; set; }
        }
    }
}