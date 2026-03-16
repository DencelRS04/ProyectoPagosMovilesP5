using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.Shared.DTOs.Roles;

namespace PagosMoviles.AdminWeb.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly IRolesService _service;
        public IndexModel(IRolesService service) => _service = service;

        public List<RolDto> Roles { get; set; } = new();
        public string? MensajeError { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                return RedirectToPage("/Account/Login");

            try
            {
                var todos = await _service.ObtenerRoles();
                Roles = string.IsNullOrWhiteSpace(Buscar)
                    ? todos
                    : todos.Where(r =>
                        r.RolId.ToString().Contains(Buscar) ||
                        r.Nombre.Contains(Buscar, StringComparison.OrdinalIgnoreCase))
                      .ToList();
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }
            catch
            {
                MensajeError = "No se pudo cargar la lista de roles.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                return RedirectToPage("/Account/Login");

            try
            {
                await _service.EliminarRol(id);
                TempData["Mensaje"] = "Rol eliminado correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar el rol.";
            }

            return RedirectToPage();
        }
    }
}
