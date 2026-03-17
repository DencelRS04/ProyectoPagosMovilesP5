using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PagosMoviles.AdminWeb.Pages.Pantallas
{
    public class DeleteModel : PageModel
    {
        private readonly HttpClient _http;

        public DeleteModel(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("gateway");
        }

        public async Task<IActionResult> OnGet(int id)
        {
            await _http.DeleteAsync($"/pantallas/{id}");

            return RedirectToPage("Index");
        }
    }
}