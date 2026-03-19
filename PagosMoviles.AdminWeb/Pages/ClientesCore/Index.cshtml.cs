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

        public async Task OnGetAsync()
        {
            Clientes = await _service.ListarAsync();
        }
    }
}