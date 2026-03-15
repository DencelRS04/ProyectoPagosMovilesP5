using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Models.Desinscripcion;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.PortalWeb.Pages.Desinscripcion
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpFactory;

        public IndexModel(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        [BindProperty]
        public DesinscripcionFormModel Formulario { get; set; } = new();

        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpFactory.CreateClient("InscripcionApi");

            var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var body = JsonSerializer.Serialize(Formulario, opciones);
            var content = new StringContent(body, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            var response = await client.PostAsync("accounts/unsubscribe", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                MensajeExito = "Te has desinscrito exitosamente de Pagos Móviles.";
                Formulario = new(); // limpiar formulario
                ModelState.Clear();
            }
            else
            {
                var resultado = JsonSerializer.Deserialize<ApiRespuesta>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                MensajeError = resultado?.Descripcion ?? "No se pudo procesar la desinscripción.";
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