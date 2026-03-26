using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.Shared.DTOs.Pantallas;
using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Pages.Pantallas
{
    public class IndexModel : PageModel
    {
        private readonly IPantallasService _service;

        public IndexModel(IPantallasService service)
        {
            _service = service;
        }

        public List<PantallaDto> Pantallas { get; set; } = new();
        public string? MensajeError { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        // CREAR
        [BindProperty]
        public PantallaCreateDtoValidado NuevaPantalla { get; set; } = new();

        // EDITAR
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public PantallaEditDtoValidado Pantalla { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            await CargarPantallas();
            return Page();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("USUARIO_SESION")))
                return RedirectToPage("/Auth/Login");

            var dto = new PantallaCreateDto
            {
                Identificador = Request.Form["NuevaPantalla.Identificador"],
                Nombre = Request.Form["NuevaPantalla.Nombre"],
                Descripcion = Request.Form["NuevaPantalla.Descripcion"],
                Ruta = Request.Form["NuevaPantalla.Ruta"]
            };

            try
            {
                await _service.CrearPantalla(dto);
                TempData["Mensaje"] = "Pantalla creada correctamente.";
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
            var dto = new PantallaCreateDto
            {
                Identificador = Request.Form["Pantalla.Identificador"],
                Nombre = Request.Form["Pantalla.Nombre"],
                Descripcion = Request.Form["Pantalla.Descripcion"],
                Ruta = Request.Form["Pantalla.Ruta"]
            };

            try
            {
                await _service.ActualizarPantalla(id, dto);
                TempData["Mensaje"] = "Pantalla actualizada correctamente.";
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
                await _service.EliminarPantalla(id);
                TempData["Mensaje"] = "Pantalla eliminada correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar la pantalla.";
            }

            return RedirectToPage();
        }

        private async Task CargarPantallas()
        {
            try
            {
                var todas = await _service.ObtenerPantallas();

                Pantallas = string.IsNullOrWhiteSpace(Buscar)
                    ? todas
                    : todas.Where(p =>
                        p.Nombre.Contains(Buscar, StringComparison.OrdinalIgnoreCase) ||
                        p.Identificador.Contains(Buscar, StringComparison.OrdinalIgnoreCase))
                      .ToList();
            }
            catch (HttpRequestException ex) when
                (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                MensajeError = "Sesión no autorizada.";
                Pantallas = new List<PantallaDto>();
            }
            catch
            {
                MensajeError = "No se pudo cargar la lista de pantallas. Verifique la conexión.";
                Pantallas = new List<PantallaDto>();
            }
        }
    }

    public class PantallaCreateDtoValidado
    {
        [Required(ErrorMessage = "El identificador es obligatorio.")]
        public string Identificador { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ruta es obligatoria.")]
        public string Ruta { get; set; } = string.Empty;
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