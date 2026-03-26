using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Services.Transferencias;
using PagosMoviles.PortalWeb.Models.Transferencias;
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
        [Required(ErrorMessage = "El teléfono origen es obligatorio.")]
        public string TelefonoOrigen { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "El nombre origen es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre origen no puede superar 100 caracteres.")]
        public string NombreOrigen { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "El teléfono destino es obligatorio.")]
        public string TelefonoDestino { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La entidad destino es obligatoria.")]
        public string EntidadDestino { get; set; } = string.Empty;

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

            TelefonoOrigen = (TelefonoOrigen ?? string.Empty).Trim();
            NombreOrigen = (NombreOrigen ?? string.Empty).Trim();
            TelefonoDestino = (TelefonoDestino ?? string.Empty).Trim();
            EntidadDestino = (EntidadDestino ?? string.Empty).Trim();
            Descripcion = (Descripcion ?? string.Empty).Trim();

            if (!Regex.IsMatch(TelefonoOrigen, @"^(?:2|4|5|6|7|8)\d{7}$"))
                ModelState.AddModelError(nameof(TelefonoOrigen), "El teléfono origen debe tener 8 dígitos válidos.");

            if (!Regex.IsMatch(TelefonoDestino, @"^(?:2|4|5|6|7|8)\d{7}$"))
                ModelState.AddModelError(nameof(TelefonoDestino), "El teléfono destino debe tener 8 dígitos válidos.");

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var dto = new TransferenciaRequestDto
                {
                    EntidadOrigen = "BNCR",
                    TelefonoOrigen = TelefonoOrigen,
                    NombreOrigen = NombreOrigen,
                    TelefonoDestino = TelefonoDestino,
                    EntidadDestino = EntidadDestino,
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