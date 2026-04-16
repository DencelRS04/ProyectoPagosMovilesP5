using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.AdminWeb.Models.Usuarios;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PagosMoviles.AdminWeb.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpFactory;

        public IndexModel(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public List<UsuarioViewModel> Usuarios { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty]
        public UsuarioFormModel Formulario { get; set; } = new();

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }

        private HttpClient CrearClienteConToken()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);
            var token = usuario?.AccessToken;
            var client = _httpFactory.CreateClient("UsuarioApi");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuario == null || string.IsNullOrEmpty(usuario.AccessToken))
                return Redirect("/");

            var client = CrearClienteConToken();
            var response = await client.GetAsync("gateway/admin/user");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ApiRespuesta<List<UsuarioViewModel>>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Usuarios = resultado?.Datos ?? new();

                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    var b = Busqueda.ToLower();
                    Usuarios = Usuarios
                        .Where(u =>
                            (!string.IsNullOrWhiteSpace(u.NombreCompleto) && u.NombreCompleto.ToLower().Contains(b)) ||
                            (!string.IsNullOrWhiteSpace(u.Identificacion) && u.Identificacion.ToLower().Contains(b)))
                        .ToList();
                }
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                MensajeError = $"Error al cargar usuarios. ({(int)response.StatusCode}) {errorBody}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            bool esNuevo = !Formulario.UsuarioId.HasValue || Formulario.UsuarioId == 0;

            if (esNuevo && string.IsNullOrWhiteSpace(Formulario.Password))
                ModelState.AddModelError("Formulario.Password", "La contraseña es obligatoria al crear un usuario.");

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var client = CrearClienteConToken();
            var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var body = JsonSerializer.Serialize(Formulario, opciones);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (!esNuevo)
                response = await client.PutAsync($"gateway/admin/user/{Formulario.UsuarioId}", content);
            else
                response = await client.PostAsync("gateway/admin/user", content);

            if (response.IsSuccessStatusCode)
            {
                MensajeExito = "Usuario guardado correctamente.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                MensajeError = $"Error al guardar el usuario. ({(int)response.StatusCode}): {errorBody}";
            }

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var client = CrearClienteConToken();
            var response = await client.DeleteAsync($"gateway/admin/user/{id}");

            if (response.IsSuccessStatusCode)
                MensajeExito = "Usuario eliminado correctamente.";
            else
                MensajeError = "No se pudo eliminar el usuario.";

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