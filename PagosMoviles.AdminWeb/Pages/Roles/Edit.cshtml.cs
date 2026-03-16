using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.Shared.DTOs.Pantallas;
using PagosMoviles.Shared.DTOs.Roles;
using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Pages.Roles
{
    public class EditModel : PageModel
    {
        private readonly IRolesService _rolesService;
        private readonly IPantallasService _pantallasService;

        public EditModel(IRolesService rolesService, IPantallasService pantallasService)
        {
            _rolesService = rolesService;
            _pantallasService = pantallasService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } //← coincide con {id:int} en la ruta

        [BindProperty]
        public RolCreateDtoValidado Rol { get; set; } = new RolCreateDtoValidado();

        [BindProperty]
        public List<int> PantallasSeleccionadas { get; set; } = new List<int>();

        public List<PantallaDto> Pantallas { get; set; } = new List<PantallaDto>();

        public string MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
             if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                 return RedirectToPage("/Account/Login");

            await CargarPantallas();

            try
            {
                var rol = await _rolesService.ObtenerRol(Id);
                if (rol == null)
                {
                    TempData["Error"] = "Rol no encontrado.";
                    return RedirectToPage("Index");
                }
                Rol = new RolCreateDtoValidado { Nombre = rol.Nombre };
            }
            catch
            {
                MensajeError = "No se pudo cargar el rol.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
             if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                 return RedirectToPage("/Account/Login");

            await CargarPantallas();

            if (!ModelState.IsValid) return Page();

            try
            {
                var dto = new RolCreateDto
                {
                    Nombre = Rol.Nombre,
                    Pantallas = PantallasSeleccionadas
                };

                await _rolesService.ActualizarRol(Id, dto);
                TempData["Mensaje"] = "Rol actualizado correctamente.";
                return RedirectToPage("Index");
            }
            catch
            {
                MensajeError = "No se pudo actualizar el rol.";
                return Page();
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