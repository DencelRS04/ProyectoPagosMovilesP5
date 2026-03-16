using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.Shared.DTOs.Pantallas;

namespace PagosMoviles.AdminWeb.Pages.Pantallas
{
    public class IndexModel : PageModel
    {
        private readonly IPantallasService _service;

        public IndexModel(IPantallasService service) => _service = service;

        public List<PantallaDto> Pantallas { get; set; } = new();
        public string? MensajeError { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Buscar { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                return RedirectToPage("/Account/Login");

            try
            {
                var todas = await _service.ObtenerPantallas();

                // Filtro por nombre si hay búsqueda
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
                return RedirectToPage("/Account/Login");
            }
            catch
            {
                MensajeError = "No se pudo cargar la lista de pantallas. Verifique la conexión.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt_token")))
                return RedirectToPage("/Account/Login");

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
    }
}
