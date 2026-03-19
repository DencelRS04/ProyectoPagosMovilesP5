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
                try
                {
                    Movimientos = await _service.ObtenerUltimosMovimientos(Telefono);
                    // ✅ Log temporal
                    Console.WriteLine($"[DEBUG] Movimientos count: {Movimientos?.Count ?? -1}");
                }
                catch (Exception ex)
                {
                    ErrorMensaje = ex.Message;
                    Console.WriteLine($"[DEBUG ERROR] {ex.Message}");
                }
            }
        }
    }
}