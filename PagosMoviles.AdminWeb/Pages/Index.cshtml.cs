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
        private static int intentosFallidos = 0;

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
                Input.Usuario = string.Empty;
                Input.Contrasena = string.Empty;
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

            if (!result.Item1)
            {
                var mensajeServicio = (result.Item2 ?? "").ToLower();

                if (mensajeServicio.Contains("bloqueado"))
                {
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    Input.Usuario = string.Empty;
                    Input.Contrasena = string.Empty;
                    return Page();
                }

                intentosFallidos++;

                if (intentosFallidos >= 3)
                {
                    Input.MensajeError = "El usuario se bloqueó por fallar 3 intentos.";
                    Input.Usuario = string.Empty;
                    Input.Contrasena = string.Empty;
                    return Page();
                }

                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                Input.Usuario = string.Empty;
                Input.Contrasena = string.Empty;
                return Page();
            }

            var usuario = result.Item3;
            usuario.FotoPerfil = usuario.FotoPerfil ?? "";
            usuario.ColorAvatar = string.IsNullOrWhiteSpace(usuario.ColorAvatar) ? "#4285F4" : usuario.ColorAvatar;
            intentosFallidos = 0;

            if (result.Item3.RolId != ADMIN_ROL)
            {
                Input.MensajeError = "No tiene permisos para acceder al portal administrativo.";
                Input.Usuario = string.Empty;
                Input.Contrasena = string.Empty;
                return Page();
            }

            SessionHelper.LimpiarSesion(HttpContext.Session);
            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, result.Item3);
            // ✅ Guardar token con la clave que usa BearerTokenHandler
            HttpContext.Session.SetString("AccessToken", result.Item3.AccessToken ?? "");

            return Redirect("/Home/Index");
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/");
        }
    }
}