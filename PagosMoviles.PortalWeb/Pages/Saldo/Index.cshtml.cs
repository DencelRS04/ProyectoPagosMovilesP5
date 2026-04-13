using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.Shared.DTOs.Saldo;
using PagosMoviles.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PagosMoviles.PortalWeb.Pages.Saldo
{
    public class IndexModel : PageModel
    {
        private readonly ISaldoService _service;

        public IndexModel(ISaldoService service) => _service = service;

        [BindProperty]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        public SaldoResponseDto? Resultado { get; set; }
        public string? MensajeError { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            if (!ModelState.IsValid) return Page();

            try
            {
                // Obtener identificacion del usuario logueado desde la BD
                var json = HttpContext.Session.GetString("USUARIO_SESION");
                var usuario = JsonSerializer.Deserialize<UsuarioSesionModel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var identificacion = string.Empty;
                var connStr = "Server=138.59.135.33;Database=PagosMoviles;User Id=denceljrs04;Password=denceljasan2004;TrustServerCertificate=True;Encrypt=False;";

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                await conn.OpenAsync();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT Identificacion FROM Usuario WHERE UsuarioId = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", int.Parse(usuario.UsuarioId));
                var result = await cmd.ExecuteScalarAsync();
                identificacion = result?.ToString() ?? string.Empty;

                Resultado = await _service.ConsultarSaldo(Telefono, identificacion);
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Auth/Login");
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                MensajeError = "Los datos indicados no son válidos o el cliente no está asociado a pagos móviles.";
            }
            catch (Exception ex)
            {
                MensajeError = $"No se pudo consultar el saldo: {ex.Message}";
            }

            return Page();
        }
    }
}