using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.ClientesCore;
using PagosMoviles.AdminWeb.Services.ClientesCore;

namespace PagosMoviles.AdminWeb.Pages.ClientesCore
{
    public class IndexModel : PageModel
    {
        private readonly ClientesCoreService _service;

        public IndexModel(ClientesCoreService service)
        {
            _service = service;
        }

        public List<ClienteViewModel> Clientes { get; set; } = new();

        [BindProperty]
        public ClienteViewModel Cliente { get; set; } = new();

        [BindProperty]
        public ClienteViewModel ClienteEditar { get; set; } = new();

        [BindProperty]
        public ClienteViewModel ClienteEliminar { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clientes = await _service.ListarAsync();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (!ModelState.IsValid)
            {
                Clientes = await _service.ListarAsync();
                return Page();
            }

            var resultado = await _service.CrearAsync(Cliente);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                Clientes = await _service.ListarAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Clientes = await _service.ListarAsync();
                return Page();
            }

            var resultado = await _service.ActualizarAsync(id, ClienteEditar);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                Clientes = await _service.ListarAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCargarEditarAsync(int id)
        {
            var cliente = await _service.ObtenerPorIdAsync(id);
            if (cliente == null)
                return RedirectToPage();

            ClienteEditar = cliente;
            Clientes = await _service.ListarAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCargarEliminarAsync(int id)
        {
            var cliente = await _service.ObtenerPorIdAsync(id);
            if (cliente == null)
                return RedirectToPage();

            ClienteEliminar = cliente;
            Clientes = await _service.ListarAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _service.EliminarAsync(id);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                Clientes = await _service.ListarAsync();
                return Page();
            }

            return RedirectToPage();
        }
    }
}