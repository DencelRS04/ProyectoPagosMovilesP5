using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Services.Entidades;

namespace PagosMoviles.AdminWeb.Pages.Entidades
{
    public class IndexModel : PageModel
    {
        private readonly EntidadService _service;

        public List<EntidadViewModel> Entidades { get; set; } = new();

        [BindProperty]
        public EntidadCreateModel Entidad { get; set; } = new();

        [BindProperty]
        public EntidadEditModel EntidadEditar { get; set; } = new();

        public string? ErrorMensaje { get; set; }

        public IndexModel(EntidadService service)
        {
            _service = service;
        }

        public async Task OnGet()
        {
            Entidades = await _service.ListarAsync();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (!ModelState.IsValid)
            {
                Entidades = await _service.ListarAsync();
                return Page();
            }

            var (ok, mensaje) = await _service.CrearAsync(Entidad);

            if (!ok)
            {
                ErrorMensaje = mensaje;
                Entidades = await _service.ListarAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (!ModelState.IsValid)
            {
                Entidades = await _service.ListarAsync();
                return Page();
            }

            if (EntidadEditar?.EntidadId == 0)
            {
                ModelState.AddModelError(string.Empty, "El ID de entidad no fue recibido.");
                Entidades = await _service.ListarAsync();
                return Page();
            }

            await _service.ActualizarAsync(EntidadEditar.EntidadId, EntidadEditar);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCargarEditarAsync(int id)
        {
            var entidadVm = await _service.ObtenerPorIdAsync(id);

            if (entidadVm == null)
            {
                TempData["Error"] = $"ID incorrecto: no existe una entidad con el ID {id}.";
                return RedirectToPage();
            }

            EntidadEditar = new EntidadEditModel
            {
                EntidadId = entidadVm.EntidadId,
                CodigoEntidad = entidadVm.CodigoEntidad,
                NombreInstitucion = entidadVm.NombreInstitucion
            };

            Entidades = await _service.ListarAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminar(int id)
        {
            await _service.EliminarAsync(id);
            return RedirectToPage();
        }
    }
}