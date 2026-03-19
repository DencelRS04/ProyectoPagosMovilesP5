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
            Input = new LoginInputModel();

            var mensajeExpirado = HttpContext.Session.GetString("SessionExpiredMessage");

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Input.MensajeError = "Debe completar todos los campos.";
                return Page();
            }

            int intentos = HttpContext.Session.GetInt32("IntentosLogin") ?? 0;

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

            if (result == null || !result.Item1 || result.Item3 == null)
            {
                var mensaje = (result?.Item2 ?? "").ToLower();

                // 🔴 SI VIENE BLOQUEADO DESDE BD
                if (mensaje.Contains("bloqueado"))
                {
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    return Page();
                }

                intentos++;
                HttpContext.Session.SetInt32("IntentosLogin", intentos);

                if (intentos >= 3)
                {
                    Input.MensajeError = "El usuario se bloqueó por fallar 3 intentos.";
                }
                else
                {
                    Input.MensajeError = $"Usuario y/o contraseña incorrectos. Intento {intentos} de 3.";
                }

                return Page();
            }

            // ✅ LOGIN OK → RESET
            HttpContext.Session.SetInt32("IntentosLogin", 0);

            var usuario = result.Item3;

            usuario.FotoPerfil = usuario.FotoPerfil ?? "";
            usuario.ColorAvatar = string.IsNullOrWhiteSpace(usuario.ColorAvatar) ? "#4285F4" : usuario.ColorAvatar;

            // 🔥 ADMIN = 5
            if (usuario.RolId != 5)
            {
                Input.MensajeError = "No tiene permisos para acceder al portal administrativo.";
                return Page();
            }

            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuario);

            return Redirect("/Home/Index");
        }

        // 🔥 MENSAJE DE SESIÓN EXPIRADA
        public IActionResult OnPostSetSessionExpired()
        {
            HttpContext.Session.SetString(SessionKeys.SessionExpiredMessage, "La sesión expiró por inactividad.");
            return new EmptyResult();
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/Auth/Login");
        }
    }
}