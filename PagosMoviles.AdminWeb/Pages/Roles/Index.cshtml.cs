using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.Shared.DTOs.Pantallas;
using PagosMoviles.Shared.DTOs.Roles;

namespace PagosMoviles.AdminWeb.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly IRolesService _rolesService;
        private readonly IPantallasService _pantallasService;

        public IndexModel(IRolesService rolesService, IPantallasService pantallasService)
        {
            _rolesService = rolesService;
            _pantallasService = pantallasService;
        }

        public List<RolDto> Roles { get; set; } = new();
        public List<PantallaDto> Pantallas { get; set; } = new();
        public string? MensajeError { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            await CargarRoles();
            await CargarPantallas();
            return Page();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            var nombre = Request.Form["NuevoRol.Nombre"].ToString();
            var pantallasSeleccionadas = Request.Form["PantallasSeleccionadas"]
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x))
                .Where(x => x > 0)
                .ToList();

            var dto = new RolCreateDto
            {
                Nombre = nombre,
                Pantallas = pantallasSeleccionadas
            };

            try
            {
                await _rolesService.CrearRol(dto);
                TempData["Mensaje"] = "Rol creado correctamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            var id = int.Parse(Request.Form["Id"]);
            var nombre = Request.Form["Rol.Nombre"].ToString();
            var pantallasSeleccionadas = Request.Form["PantallasSeleccionadasEditar"]
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => int.Parse(x))
                .Where(x => x > 0)
                .ToList();

            var dto = new RolCreateDto
            {
                Nombre = nombre,
                Pantallas = pantallasSeleccionadas
            };

            try
            {
                await _rolesService.ActualizarRol(id, dto);
                TempData["Mensaje"] = "Rol actualizado correctamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            try
            {
                await _rolesService.EliminarRol(id);
                TempData["Mensaje"] = "Rol eliminado correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar el rol.";
            }

            return RedirectToPage();
        }

        private async Task CargarRoles()
        {
            try
            {
                var todos = await _rolesService.ObtenerRoles();

                Roles = string.IsNullOrWhiteSpace(Buscar)
                    ? todos
                    : todos.Where(r =>
                        r.RolId.ToString().Contains(Buscar) ||
                        r.Nombre.Contains(Buscar, StringComparison.OrdinalIgnoreCase))
                      .ToList();
            }
            catch
            {
                MensajeError = "No se pudo cargar la lista de roles.";
                Roles = new List<RolDto>();
            }
        }

        private async Task CargarPantallas()
        {
            try
            {
                Pantallas = await _pantallasService.ObtenerPantallas();
            }
            catch
            {
                Pantallas = new List<PantallaDto>();
            }
        }
    }
}