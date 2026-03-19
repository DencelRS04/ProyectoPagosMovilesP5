using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Services.Entidades;

namespace PagosMoviles.AdminWeb.Pages.Entidades
{
    public class CreateModel : PageModel
    {
        private readonly EntidadService _service;

        [BindProperty]
        public EntidadCreateModel Entidad { get; set; }

        // ✅ Para mostrar el error en la página
        public string? ErrorMensaje { get; set; }

        public CreateModel(EntidadService service)
        {
            _service = service;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var (ok, mensaje) = await _service.CrearAsync(Entidad);

            if (!ok)
            {
                // ✅ Mostrar el mensaje de error en la página sin crash
                ErrorMensaje = mensaje;
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}