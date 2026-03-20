using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.Shared.DTOs.Pantallas;
using PagosMoviles.Shared.DTOs.Roles;
using System.ComponentModel.DataAnnotations;

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

        // ===== CREAR =====
        [BindProperty]
        public RolCreateDtoValidado NuevoRol { get; set; } = new();

        [BindProperty]
        public List<int> PantallasSeleccionadas { get; set; } = new();

        // ===== EDITAR =====
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public RolCreateDtoValidado Rol { get; set; } = new();

        [BindProperty]
        public List<int> PantallasSeleccionadasEditar { get; set; } = new();

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

            await CargarRoles();
            await CargarPantallas();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var dto = new RolCreateDto
                {
                    Nombre = NuevoRol.Nombre,
                    Pantallas = PantallasSeleccionadas
                };

                await _rolesService.CrearRol(dto);
                TempData["Mensaje"] = "Rol creado correctamente.";
                return RedirectToPage();
            }
            catch
            {
                MensajeError = "No se pudo crear el rol.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            await CargarRoles();
            await CargarPantallas();

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var dto = new RolCreateDto
                {
                    Nombre = Rol.Nombre,
                    Pantallas = PantallasSeleccionadasEditar
                };

                await _rolesService.ActualizarRol(Id, dto);
                TempData["Mensaje"] = "Rol actualizado correctamente.";
                return RedirectToPage();
            }
            catch
            {
                MensajeError = "No se pudo actualizar el rol.";
                return Page();
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
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                RedirectToPage("/Account/Login");
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

    public class RolCreateDtoValidado
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—0-9 ]+$",
            ErrorMessage = "Solo letras, n˙meros y espacios.")]
        public string Nombre { get; set; } = string.Empty;
    }
}