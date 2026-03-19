using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Models.Desinscripcion;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.PortalWeb.Pages.Desinscripcion
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IndexModel(IHttpClientFactory httpFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpFactory = httpFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public DesinscripcionFormModel Formulario { get; set; } = new();

        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        public IActionResult OnGet()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);
            if (usuario == null)
                return Redirect("/");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);
            if (usuario == null)
                return Redirect("/");

            var client = _httpFactory.CreateClient("GatewayApi");

            if (!string.IsNullOrWhiteSpace(usuario.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", usuario.AccessToken);
            }

            var opciones = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var body = JsonSerializer.Serialize(Formulario, opciones);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "gateway/admin/core/accounts/unsubscribe",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    MensajeExito = "Te has desinscrito exitosamente de Pagos Móviles.";
                }
                else
                {
                    try
                    {
                        var resultadoOk = JsonSerializer.Deserialize<ApiRespuesta>(
                            responseBody,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        MensajeExito = resultadoOk?.Descripcion ?? "Te has desinscrito exitosamente de Pagos Móviles.";
                    }
                    catch
                    {
                        MensajeExito = "Te has desinscrito exitosamente de Pagos Móviles.";
                    }
                }

                MensajeError = null;
                Formulario = new();
                ModelState.Clear();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    MensajeError = $"No se pudo procesar la desinscripción. HTTP {(int)response.StatusCode}.";
                }
                else
                {
                    try
                    {
                        var resultado = JsonSerializer.Deserialize<ApiRespuesta>(
                            responseBody,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        MensajeError = resultado?.Descripcion ?? $"No se pudo procesar la desinscripción. HTTP {(int)response.StatusCode}.";
                    }
                    catch
                    {
                        MensajeError = responseBody;
                    }
                }

                MensajeExito = null;
            }

            return Page();
        }

        private class ApiRespuesta
        {
            public int Codigo { get; set; }
            public string? Descripcion { get; set; }
        }
    }
}