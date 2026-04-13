using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Services.Transferencias;
using PagosMoviles.PortalWeb.Models.Transferencias;
using PagosMoviles.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PagosMoviles.PortalWeb.Pages.Transferencias
{
    public class IndexModel : PageModel
    {
        private readonly ITransferenciaService _service;

        public IndexModel(ITransferenciaService service) => _service = service;

        [BindProperty]
        [Required(ErrorMessage = "El teléfono destino es obligatorio.")]
        public string TelefonoDestino { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1, 100000, ErrorMessage = "El monto debe ser entre 1 y 100,000.")]
        public decimal Monto { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(25, ErrorMessage = "La descripción no puede superar 25 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public TransferenciaResponseDto? Resultado { get; set; }
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

            TelefonoDestino = (TelefonoDestino ?? string.Empty).Trim();
            Descripcion = (Descripcion ?? string.Empty).Trim();

            if (!Regex.IsMatch(TelefonoDestino, @"^(?:2|4|5|6|7|8)\d{7}$"))
                ModelState.AddModelError(nameof(TelefonoDestino), "El teléfono destino debe tener 8 dígitos válidos.");

            if (!ModelState.IsValid)
                return Page();

            try
            {
                // Obtener datos del usuario logueado
                var json = HttpContext.Session.GetString("USUARIO_SESION");
                var usuario = JsonSerializer.Deserialize<UsuarioSesionModel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Obtener teléfono y nombre del usuario desde la BD
                var telefonoOrigen = string.Empty;
                var nombreOrigen = string.Empty;
                var entidadDestino = string.Empty;

                var connStr = "Server=138.59.135.33;Database=PagosMoviles;User Id=denceljrs04;Password=denceljasan2004;TrustServerCertificate=True;Encrypt=False;";

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                await conn.OpenAsync();

                // Obtener teléfono y nombre del origen
                using var cmdOrigen = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT Telefono, NombreCompleto FROM Usuario WHERE UsuarioId = @Id", conn);
                cmdOrigen.Parameters.AddWithValue("@Id", int.Parse(usuario.UsuarioId));
                using var readerOrigen = await cmdOrigen.ExecuteReaderAsync();
                if (await readerOrigen.ReadAsync())
                {
                    telefonoOrigen = readerOrigen.GetString(0);
                    nombreOrigen = readerOrigen.GetString(1);
                }
                await readerOrigen.CloseAsync();

                // Obtener entidad destino desde PagoMovil del destino
                using var cmdDestino = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT TOP 1 CodigoEntidad FROM EntidadBancaria", conn);
                var resultDestino = await cmdDestino.ExecuteScalarAsync();
                entidadDestino = resultDestino?.ToString() ?? "001";

                var dto = new TransferenciaRequestDto
                {
                    EntidadOrigen = "BNCR",
                    TelefonoOrigen = telefonoOrigen,
                    NombreOrigen = nombreOrigen,
                    TelefonoDestino = TelefonoDestino,
                    EntidadDestino = entidadDestino,
                    Monto = Monto,
                    Descripcion = Descripcion
                };

                Resultado = await _service.RealizarTransferencia(dto);
            }
            catch (HttpRequestException ex)
            {
                MensajeError = ExtraerMensajeError(ex.Message) ?? ex.Message;
            }
            catch (Exception ex)
            {
                MensajeError = $"Error inesperado: {ex.Message}";
            }

            return Page();
        }

        private string? ExtraerMensajeError(string raw)
        {
            try
            {
                var error = JsonSerializer.Deserialize<TransferenciaResponseDto>(
                    raw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return error?.Descripcion;
            }
            catch
            {
                return null;
            }
        }
    }
}