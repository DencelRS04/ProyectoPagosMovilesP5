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

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

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
            try
            {
                Entidades = await _service.ListarAsync();

                if (!string.IsNullOrWhiteSpace(Busqueda))
                {
                    var b = Busqueda.ToLower();

                    Entidades = Entidades
                        .Where(e =>
                            (!string.IsNullOrWhiteSpace(e.CodigoEntidad) && e.CodigoEntidad.ToLower().Contains(b)) ||
                            (!string.IsNullOrWhiteSpace(e.NombreInstitucion) && e.NombreInstitucion.ToLower().Contains(b)))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMensaje = $"No se pudieron cargar las entidades. {ex.Message}";
                Entidades = new List<EntidadViewModel>();
            }
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (!ModelState.IsValid)
            {
                Entidades = await _service.ListarAsync();
                return Page();
            }

            try
            {
                var (ok, mensaje) = await _service.CrearAsync(Entidad);

                if (!ok)
                {
                    ErrorMensaje = mensaje;
                    Entidades = await _service.ListarAsync();
                    return Page();
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMensaje = $"No se pudo crear la entidad. {ex.Message}";
                Entidades = await _service.ListarAsync();
                return Page();
            }
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

            try
            {
                await _service.ActualizarAsync(EntidadEditar.EntidadId, EntidadEditar);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMensaje = ex.Message;
                Entidades = await _service.ListarAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCargarEditarAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo cargar la entidad. {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEliminar(int id)
        {
            try
            {
                var (ok, mensaje) = await _service.EliminarAsync(id);

                if (!ok)
                {
                    TempData["Error"] = mensaje;
                    return RedirectToPage();
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudo eliminar la entidad. {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}