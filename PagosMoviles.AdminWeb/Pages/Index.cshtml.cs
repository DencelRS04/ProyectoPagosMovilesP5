using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.AdminWeb.Models.Auth;
using PagosMoviles.AdminWeb.Services.Auth;

namespace PagosMoviles.AdminWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuthService _authService;
        private const int ADMIN_ROL = 5;

        public IndexModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        public string Mensaje { get; set; } = string.Empty;

        public void OnGet()
        {
            Input = new LoginInputModel();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Input.Usuario = (Input.Usuario ?? "").Trim();
            Input.Contrasena = (Input.Contrasena ?? "").Trim();

            if (string.IsNullOrWhiteSpace(Input.Usuario) || string.IsNullOrWhiteSpace(Input.Contrasena))
            {
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);
            if (result != null && !result.Item1 && result.Item2 == "BLOQUEADO")
            {
                Input.MensajeError = "El usuario se encuentra bloqueado por demasiados intentos fallidos.";
                return Page();
            }

            if (result == null || !result.Item1 || result.Item3 == null)
            {
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                return Page();
            }


            if (result.Item3.RolId != ADMIN_ROL)
            {
                Input.MensajeError = "No tiene permisos para acceder al portal administrativo.";
                return Page();
            }

            SessionHelper.LimpiarSesion(HttpContext.Session);
            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, result.Item3);

            return Redirect("/Home/Index");
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/");
        }
    }
}