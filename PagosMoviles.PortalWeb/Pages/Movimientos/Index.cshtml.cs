using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Models.Movimientos;

namespace PagosMoviles.PortalWeb.Pages.Movimientos
{
    public class IndexModel : PageModel
    {
        private readonly MovimientoService _service;

        public List<MovimientosViewModels> Movimientos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Telefono { get; set; }

        public IndexModel(MovimientoService service)
        {
            _service = service;
        }

        public async Task OnGet()
        {
            if (!string.IsNullOrEmpty(Telefono))
            {
                // Llamada al servicio SRV11 según lo requerido
                Movimientos = await _service.ObtenerUltimosMovimientos(Telefono);

                // Asegurar que solo se muestren los últimos 5
                if (Movimientos != null && Movimientos.Count > 5)
                {
                    Movimientos = Movimientos.Take(5).ToList();
                }
            }
        }
    }
}