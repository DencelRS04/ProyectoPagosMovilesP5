using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.Shared.DTOs.Pantallas;
using PagosMoviles.Shared.DTOs.Roles;
using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Pages.Roles
{
    public class CreateModel : PageModel
    {
        private readonly IRolesService _rolesService;
        private readonly IPantallasService _pantallasService;

        public CreateModel(IRolesService rolesService, IPantallasService pantallasService)
        {
            _rolesService = rolesService;
            _pantallasService = pantallasService;
        }

        [BindProperty]
        public RolCreateDtoValidado NuevoRol { get; set; } = new();

        // IDs de pantallas seleccionadas con checkboxes
        [BindProperty]
        public List<int> PantallasSeleccionadas { get; set; } = new();

        // Lista de todas las pantallas para mostrar los checkboxes
        public List<PantallaDto> Pantallas { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            await CargarPantallas();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            await CargarPantallas(); //recargar para mostrar si hay error

            if (!ModelState.IsValid) return Page();

            try
            {
                var dto = new RolCreateDto
                {
                    Nombre = NuevoRol.Nombre,
                    Pantallas = PantallasSeleccionadas //← IDs seleccionados
                };

                await _rolesService.CrearRol(dto);
                TempData["Mensaje"] = "Rol creado correctamente.";
                return RedirectToPage("Index");
            }
            catch
            {
                MensajeError = "No se pudo crear el rol.";
                return Page();
            }
        }

        private async Task CargarPantallas()
        {
            try
            {
                Pantallas = await _pantallasService.ObtenerPantallas();
                Console.WriteLine($"Pantallas cargadas: {Pantallas.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando pantallas: {ex.Message}");
                Pantallas = new List<PantallaDto>();
            }
        }
    }

    public class RolCreateDtoValidado
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9 ]+$",
            ErrorMessage = "Solo letras, números y espacios.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
