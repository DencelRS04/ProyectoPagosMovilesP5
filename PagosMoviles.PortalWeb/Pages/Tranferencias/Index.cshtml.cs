using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Services.Transferencias;
using PagosMoviles.Shared.DTOs.Transferencias;
using System.ComponentModel.DataAnnotations;

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
            // if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
            //     return RedirectToPage("/Account/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
            //     return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid) return Page();

            try
            {
                // NombreOrigen viene del formulario — el cliente lo ingresa
                // En una implementación completa se obtendría del core bancario
                if (string.IsNullOrWhiteSpace(NombreOrigen))
                    NombreOrigen = "Cliente";

                var dto = new TransferenciaRequestDto
                {
                    TelefonoOrigen = TelefonoOrigen,
                    NombreOrigen = NombreOrigen,
                    TelefonoDestino = TelefonoDestino,
                    EntidadDestino = EntidadDestino,
                    Monto = Monto,
                    Descripcion = Descripcion
                };

                Resultado = await _service.RealizarTransferencia(dto);
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // return RedirectToPage("/Account/Login");
                MensajeError = "No autorizado.";
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                MensajeError = "Datos inválidos. Verifique el teléfono origen esté afiliado a pagos móviles.";
            }
            catch
            {
                MensajeError = "No se pudo realizar la transferencia. Intente de nuevo.";
            }

            return Page();
        }
    }
}
