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
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        private const int MAX_INTENTOS = 3;

        public void OnGet()
        {
            if (HttpContext.Session.GetString("UsuarioId") != null)
            {
                Response.Redirect("/Home/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Input.MensajeError = "Debe completar todos los campos.";
                return Page();
            }

            int intentos = HttpContext.Session.GetInt32("IntentosLogin") ?? 0;

            if (intentos >= MAX_INTENTOS)
            {
                Input.MensajeError = "Usuario bloqueado por demasiados intentos fallidos.";
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);
            if (result != null && !result.Item1 && result.Item2 == "BLOQUEADO")
            {
                Input.MensajeError = "El usuario está bloqueado por demasiados intentos fallidos.";
                return Page();
            }

            if (result == null || !result.Item1 || result.Item3 == null)
            {
                intentos++;
                HttpContext.Session.SetInt32("IntentosLogin", intentos);

                Input.MensajeError = $"Usuario y/o contraseña incorrectos. Intento {intentos} de {MAX_INTENTOS}.";
                return Page();
            }


            var usuario = result.Item3;

            if (usuario.RolId != Roles.Admin)
            {

                Console.WriteLine("ROL RECIBIDO: " + usuario.RolId);
                Input.MensajeError = "No tiene permisos para acceder al portal administrativo.";
                return Page();
            }

            HttpContext.Session.SetInt32("IntentosLogin", 0);

            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuario);

            return Redirect("/Home/Index");
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/Auth/Login");
        }
    }
}