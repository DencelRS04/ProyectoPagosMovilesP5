using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Models.Afiliacion;
using PagosMoviles.PortalWeb.Services.Afiliacion;

namespace PagosMoviles.PortalWeb.Pages.Afiliacion
{
    public class IndexModel : PageModel
    {
        private readonly AfiliacionService _service;

        public IndexModel(AfiliacionService service)
        {
            _service = service;
        }

        [BindProperty]
        public AfiliacionViewModel Afiliacion { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostValidarClienteAsync()
        {
            ModelState.Remove("Afiliacion.NumeroCuenta");
            ModelState.Remove("Afiliacion.Telefono");

            if (string.IsNullOrWhiteSpace(Afiliacion.Identificacion))
            {
                ModelState.AddModelError("Afiliacion.Identificacion", "La identificación es obligatoria");
                return Page();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Afiliacion.Identificacion, @"^\d{9}$"))
            {
                ModelState.AddModelError("Afiliacion.Identificacion", "La identificación debe tener 9 dígitos");
                return Page();
            }

            Afiliacion.ClienteExiste = await _service.ClienteExisteAsync(Afiliacion.Identificacion);

            if (!Afiliacion.ClienteExiste)
            {
                ModelState.AddModelError(string.Empty, "El cliente no existe en el core bancario");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existe = await _service.ClienteExisteAsync(Afiliacion.Identificacion);

            if (!existe)
            {
                ModelState.AddModelError(string.Empty, "El cliente no existe en el core bancario");
                return Page();
            }

            var resultado = await _service.RegistrarAsync(Afiliacion);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return Page();
            }

            Mensaje = resultado.mensaje;
            ModelState.Clear();
            Afiliacion = new AfiliacionViewModel();

            return RedirectToPage();
        }
    }
}