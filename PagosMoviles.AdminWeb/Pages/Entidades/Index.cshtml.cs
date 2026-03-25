using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Entidades;
using PagosMoviles.AdminWeb.Services.Entidades;

namespace PagosMoviles.AdminWeb.Pages.Entidades
{
    public class IndexModel : PageModel
    {
        private readonly EntidadService _service;

        public IndexModel(EntidadService service)
        {
            _service = service;
        }

        public List<EntidadViewModel> Entidades { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty]
        public EntidadCreateModel Entidad { get; set; } = new();

        [BindProperty]
        public EntidadEditModel EntidadEditar { get; set; } = new();

        public async Task OnGetAsync()
        {
            var lista = await _service.ListarAsync();

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                lista = lista.Where(e =>
                    (e.CodigoEntidad != null && e.CodigoEntidad.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)) ||
                    (e.NombreInstitucion != null && e.NombreInstitucion.Contains(Busqueda, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            Entidades = lista;
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            var (ok, mensaje) = await _service.CrearAsync(Entidad);

            if (ok)
                TempData["Success"] = "Entidad creada correctamente";
            else
                TempData["Error"] = mensaje;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            try
            {
                await _service.ActualizarAsync(EntidadEditar.EntidadId, EntidadEditar);
                TempData["Success"] = "Entidad editada correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var (ok, mensaje) = await _service.EliminarAsync(id);

            if (ok)
                TempData["Success"] = "Entidad eliminada correctamente";
            else
                TempData["Error"] = mensaje;

            return RedirectToPage();
        }
    }
}