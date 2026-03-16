using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.Shared.DTOs.Pantallas;
using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Pages.Pantallas
{
    public class CreateModel : PageModel
    {
        private readonly IPantallasService _service;

        public CreateModel(IPantallasService service) => _service = service;

        [BindProperty]
        public PantallaCreateDtoValidado NuevaPantalla { get; set; } = new();

        public string? MensajeError { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                return RedirectToPage("/Account/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid) return Page();

            try
            {
                var dto = new PantallaCreateDto
                {
                    Identificador = NuevaPantalla.Identificador,
                    Nombre = NuevaPantalla.Nombre,
                    Descripcion = NuevaPantalla.Descripcion,
                    Ruta = NuevaPantalla.Ruta
                };

                await _service.CrearPantalla(dto);
                TempData["Mensaje"] = "Pantalla creada correctamente.";
                return RedirectToPage("Index");
            }
            catch
            {
                MensajeError = "No se pudo crear la pantalla. Verifique los datos.";
                return Page();
            }
        }
    }

    //Clase con validaciones del lado del cliente
    public class PantallaCreateDtoValidado
    {
        [Required(ErrorMessage = "El identificador es obligatorio.")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9 ]+$",
            ErrorMessage = "El nombre solo puede contener letras, números y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9 ]+$",
            ErrorMessage = "La descripción solo puede contener letras, números y espacios.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ruta es obligatoria.")]
        public string Ruta { get; set; } = string.Empty;
    }
}