using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.ClientesCore;
using PagosMoviles.AdminWeb.Services.ClientesCore;

namespace PagosMoviles.AdminWeb.Pages.ClientesCore
{
    public class CreateModel : PageModel
    {
        private readonly ClientesCoreService _service;

        public CreateModel(ClientesCoreService service)
        {
            _service = service;
        }

        [BindProperty]
        public ClienteViewModel Cliente { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var resultado = await _service.CrearAsync(Cliente);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}