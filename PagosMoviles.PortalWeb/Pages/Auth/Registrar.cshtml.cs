using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.Usuarios;
using PagosMoviles.PortalWeb.Services.Auth;

namespace PagosMoviles.PortalWeb.Pages.Auth
{
    public class RegistrarModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegistrarModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public UsuarioFormModel Input { get; set; } = new UsuarioFormModel();

        public string Mensaje { get; set; } = "";
        public string Error { get; set; } = "";

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Error = "Debe completar todos los campos correctamente.";
                return Page();
            }

            var result = await _authService.RegisterAsync(Input);

            if (!result.Item1)
            {
                Error = result.Item2; // 🔥 MOSTRAR ERROR REAL
                return Page();
            }

            Mensaje = result.Item2;
            Input = new UsuarioFormModel(); // limpiar formulario

            return Page();
        }
    }
}