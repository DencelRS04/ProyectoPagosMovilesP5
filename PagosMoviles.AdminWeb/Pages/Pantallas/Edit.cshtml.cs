using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.Shared.DTOs.Pantallas;
using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Pages.Pantallas
{
    public class EditModel : PageModel
    {
        private readonly IPantallasService _service;

        public EditModel(IPantallasService service) => _service = service;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public PantallaEditDtoValidado Pantalla { get; set; } = new PantallaEditDtoValidado();

        public string MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            try
            {
                var pantalla = await _service.ObtenerPantalla(Id);
                if (pantalla == null)
                {
                    TempData["Error"] = "Pantalla no encontrada.";
                    return RedirectToPage("Index");
                }

                Pantalla = new PantallaEditDtoValidado
                {
                    Identificador = pantalla.Identificador,
                    Nombre = pantalla.Nombre,
                    Descripcion = pantalla.Descripcion,
                    Ruta = pantalla.Ruta
                };
            }
            catch
            {
                MensajeError = "No se pudo cargar la pantalla.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            if (!ModelState.IsValid) return Page();

            try
            {
                var dto = new PantallaCreateDto
                {
                    Identificador = Pantalla.Identificador,
                    Nombre = Pantalla.Nombre,
                    Descripcion = Pantalla.Descripcion,
                    Ruta = Pantalla.Ruta
                };

                await _service.ActualizarPantalla(Id, dto);
                TempData["Mensaje"] = "Pantalla actualizada correctamente.";
                return RedirectToPage("Index");
            }
            catch
            {
                MensajeError = "No se pudo actualizar la pantalla.";
                return Page();
            }
        }
    }

    public class PantallaEditDtoValidado
    {
        [Required(ErrorMessage = "El identificador es obligatorio.")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9 ]+$",
            ErrorMessage = "Solo letras, números y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9 ]+$",
            ErrorMessage = "Solo letras, números y espacios.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ruta es obligatoria.")]
        public string Ruta { get; set; } = string.Empty;
    }
}