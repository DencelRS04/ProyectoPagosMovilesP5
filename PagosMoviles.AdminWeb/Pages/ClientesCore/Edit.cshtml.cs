using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.ClientesCore;
using PagosMoviles.AdminWeb.Services.ClientesCore;

namespace PagosMoviles.AdminWeb.Pages.ClientesCore
{
    public class EditModel : PageModel
    {
        private readonly ClientesCoreService _service;

        public EditModel(ClientesCoreService service)
        {
            _service = service;
        }

        [BindProperty]
        public ClienteViewModel Cliente { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cliente = await _service.ObtenerPorIdAsync(id);
            if (cliente == null)
                return RedirectToPage("Index");

            Cliente = cliente;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
                return Page();

            var resultado = await _service.ActualizarAsync(id, Cliente);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}