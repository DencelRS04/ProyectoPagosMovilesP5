using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Cuentas;
using PagosMoviles.AdminWeb.Services.Cuentas;

namespace PagosMoviles.AdminWeb.Pages.Cuentas
{
    public class IndexModel : PageModel
    {
        private readonly CuentasService _service;

        public IndexModel(CuentasService service)
        {
            _service = service;
        }

        public List<CuentaViewModel> Cuentas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty]
        public CuentaCreateModel Cuenta { get; set; } = new();

        [BindProperty]
        public CuentaEditModel CuentaEditar { get; set; } = new();

        // ── GET ──────────────────────────────────────────────────────
        public async Task OnGetAsync()
        {
            try
            {
                // Si es número busca por clienteId
                if (!string.IsNullOrWhiteSpace(Busqueda) &&
                    int.TryParse(Busqueda.Trim(), out var clienteId))
                {
                    Cuentas = await _service.ListarPorClienteAsync(clienteId);
                }
                else
                {
                    Cuentas = await _service.ListarAsync();

                    // Filtro local por número de cuenta
                    if (!string.IsNullOrWhiteSpace(Busqueda))
                        Cuentas = Cuentas
                            .Where(c => c.NumeroCuenta.Contains(
                                Busqueda, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"No se pudieron cargar las cuentas: {ex.Message}";
            }
        }

        // ── POST Crear ───────────────────────────────────────────────
        public async Task<IActionResult> OnPostCrearAsync()
        {
            try
            {
                var (ok, mensaje) = await _service.CrearAsync(Cuenta);
                TempData[ok ? "Success" : "Error"] = mensaje;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }

        // ── POST Editar ──────────────────────────────────────────────
        public async Task<IActionResult> OnPostEditarAsync()
        {
            try
            {
                var (ok, mensaje) = await _service.ActualizarAsync(CuentaEditar.CuentaId, CuentaEditar);
                TempData[ok ? "Success" : "Error"] = mensaje;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }

        // ── POST Eliminar ─────────────────────────────────────────────
        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                var (ok, mensaje) = await _service.EliminarAsync(id);
                TempData[ok ? "Success" : "Error"] = mensaje;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage();
        }
    }
}