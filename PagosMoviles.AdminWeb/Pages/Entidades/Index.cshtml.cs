using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Services.Entidades;

namespace PagosMoviles.AdminWeb.Pages.Entidades
{
    public class IndexModel : PageModel
    {
        private readonly EntidadService _service;

        public List<EntidadViewModel> Entidades { get; set; }

        public IndexModel(EntidadService service)
        {
            _service = service;
        }

        public async Task OnGet()
        {
            var token = HttpContext.Session.GetString("token");

            Entidades = await _service.ListarAsync();
        }

        public async Task<IActionResult> OnPostEliminar(int id)
        {
            var token = HttpContext.Session.GetString("token");

            await _service.EliminarAsync(id);

            return RedirectToPage();
        }
    }
}