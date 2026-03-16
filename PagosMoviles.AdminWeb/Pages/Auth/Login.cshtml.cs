using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.AdminWeb.Models.Auth;
using PagosMoviles.AdminWeb.Services.Auth;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.AdminWeb.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public string Mensaje { get; set; }

        public void OnGet()
        {
            Input = new LoginInputModel();
            Mensaje = string.Empty;

            var mensajeExpirado = HttpContext.Session.GetString(SessionKeys.SessionExpiredMessage);
            if (!string.IsNullOrWhiteSpace(mensajeExpirado))
            {
                Mensaje = mensajeExpirado;
                HttpContext.Session.Remove(SessionKeys.SessionExpiredMessage);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input == null)
                Input = new LoginInputModel();

            Input.Usuario = (Input.Usuario ?? "").Trim();
            Input.Contrasena = (Input.Contrasena ?? "").Trim();

            if (string.IsNullOrWhiteSpace(Input.Usuario) || string.IsNullOrWhiteSpace(Input.Contrasena))
            {
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

            if (result == null || !result.Item1 || result.Item3 == null)
            {
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                return Page();
            }

            SessionHelper.LimpiarSesion(HttpContext.Session);
            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, result.Item3);

            return Redirect("/Home/Index");
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/Auth/Login");
        }
    }
}