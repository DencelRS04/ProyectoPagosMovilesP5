using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Transacciones;
using PagosMoviles.AdminWeb.Services.Reporte;

namespace PagosMoviles.AdminWeb.Pages.Transacciones
{
    public class IndexModel : PageModel
    {
        private readonly ReporteService _reporte;

        public List<TransaccionViewModel> Transacciones { get; set; } = new();
        public DateTime? Fecha { get; set; }
        public decimal Total => Transacciones.Sum(t => t.Monto);

        public IndexModel(ReporteService reporte)
        {
            _reporte = reporte;
        }

        public async Task OnGetAsync(DateTime? fecha)
        {
            Fecha = fecha;
            if (fecha.HasValue)
            {
                // ✅ Obtener token de la sesión y pasarlo al servicio
                var token = HttpContext.Session.GetString("token");
                Transacciones = await _reporte.ObtenerPorFecha(fecha.Value, token);
            }
        }
    }
}