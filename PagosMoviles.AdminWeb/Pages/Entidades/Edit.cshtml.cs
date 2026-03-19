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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entidadVm = await _service.ObtenerPorIdAsync(id);

            if (entidadVm == null)
            {
                TempData["Error"] = $"ID incorrecto: no existe una entidad con el ID {id}.";
                return RedirectToPage("Index");
            }

            Entidad = new EntidadEditModel
            {
                EntidadId = entidadVm.EntidadId,
                CodigoEntidad = entidadVm.CodigoEntidad,
                NombreInstitucion = entidadVm.NombreInstitucion
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Entidad?.EntidadId == 0)
            {
                ModelState.AddModelError("", "El ID de entidad no fue recibido.");
                return Page();
            }

            await _service.ActualizarAsync(Entidad.EntidadId, Entidad);
            return RedirectToPage("Index");
        }
    }
}