using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Models.Movimientos;

namespace PagosMoviles.PortalWeb.Pages.Movimientos
{
    public class IndexModel : PageModel
    {
        private readonly MovimientoService _service;

        public List<MovimientosViewModels> Movimientos { get; set; } = new();
        public string? ErrorMensaje { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Telefono { get; set; } = string.Empty;

        public IndexModel(MovimientoService service)
        {
            _service = service;
        }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(Telefono))
            {
                if (Telefono.Length > 8)
                {
                    ErrorMensaje = "El número de teléfono no puede tener más de 8 dígitos.";
                    return;
                }

                try
                {
                    Movimientos = await _service.ObtenerUltimosMovimientos(Telefono);
                }
                catch (UnauthorizedAccessException)
                {
                    ErrorMensaje = "Sesión expirada. Por favor inicie sesión nuevamente.";
                }
                catch (Exception ex)
                {
                    ErrorMensaje = $"Error al consultar movimientos: {ex.Message}";
                }
            }
        }
    }
}