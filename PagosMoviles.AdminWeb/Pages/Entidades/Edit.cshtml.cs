using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Services.Entidades;

namespace PagosMoviles.AdminWeb.Pages.Entidades
{
    public class EditModel : PageModel
    {
        private readonly EntidadService _service;

        [BindProperty]
        public EntidadEditModel Entidad { get; set; }

        public EditModel(EntidadService service)
        {
            _service = service;
        }

        public void OnGet(int id)
        {
            Entidad = new EntidadEditModel
            {
                Identificador = id
            };
        }

        public async Task<IActionResult> OnPost()
        {
            var token = HttpContext.Session.GetString("token");

            await _service.ActualizarAsync(Entidad.Identificador, Entidad);

            return RedirectToPage("Index");
        }
    }
}