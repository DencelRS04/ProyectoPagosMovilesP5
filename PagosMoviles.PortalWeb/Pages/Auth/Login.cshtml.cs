using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Models.Auth;
using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.PortalWeb.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public LoginModel(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
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
                Input.Contrasena = string.Empty;
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

            // 👇 USUARIO BLOQUEADO
            if (result != null && !result.Item1 && result.Item2 == "BLOQUEADO")
            {
                Input.MensajeError = "El usuario se encuentra bloqueado por demasiados intentos fallidos.";
                return Page();
            }

            if (result == null || !result.Item1 || result.Item3 == null)
            {
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                Input.Contrasena = string.Empty;
                return Page();
            }

            int rol = result.Item3.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            if (rol == 5)
            {
                return Redirect(adminUrl + "/");
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