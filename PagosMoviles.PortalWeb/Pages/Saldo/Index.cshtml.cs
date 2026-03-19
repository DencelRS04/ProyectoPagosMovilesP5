using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.Shared.DTOs.Saldo;
using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.PortalWeb.Pages.Saldo
{
    public class IndexModel : PageModel
    {
        private readonly ISaldoService _service;

        public IndexModel(ISaldoService service) => _service = service;

        [BindProperty]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Identificacion { get; set; } = string.Empty;

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
                Resultado = await _service.ConsultarSaldo(Telefono, Identificacion);
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                MensajeError = "Los datos indicados no son válidos o el cliente no está asociado a pagos móviles.";
            }
            catch
            {
                MensajeError = "No se pudo consultar el saldo. Verifique los datos e intente de nuevo.";
            }

            return Page();
        }
    }
}
