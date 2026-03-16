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

        public CreateModel(EntidadService service)
        {
            _service = service;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var token = HttpContext.Session.GetString("token");

            await _service.CrearAsync(Entidad);

            return RedirectToPage("Index");
        }
    }
}