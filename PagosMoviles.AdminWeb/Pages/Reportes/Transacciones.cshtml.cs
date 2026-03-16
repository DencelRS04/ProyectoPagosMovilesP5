using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PagosMoviles.AdminWeb.Pages.Reportes
{
    public class TransaccionesModel : PageModel
    {
        private readonly ReporteService _service;

        public List<TransaccionViewModel> Transacciones { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? Fecha { get; set; }

        public decimal Total { get; set; }

        public TransaccionesModel(ReporteService service)
        {
            _service = service;
        }

        public async Task OnGet()
        {
            if (Fecha.HasValue)
            {
                Transacciones = await _service.ObtenerPorFecha(Fecha.Value);

                Total = Transacciones.Sum(t => t.Monto);
            }
        }
    }
}
