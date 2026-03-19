using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Models.Auth;
using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.PortalWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private static int intentosFallidos = 0;
        public IndexModel(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        public string Mensaje { get; set; } = string.Empty;

        public void OnGet()
        {
            Input = new LoginInputModel();

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
                Input.Contrasena = "";
                Input.Usuario = string.Empty;
                Input.Contrasena = string.Empty;
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);
            var mensajeServicio = (result.Item2 ?? "").ToLower();

            // 🔴 usuario no registrado
            // 🔴 USUARIO NO EXISTE
            if (result.Item3 == null)
            {
                Input.MensajeError = "El usuario no está registrado.";
                Input.Usuario = string.Empty;
                Input.Contrasena = string.Empty;
                return Page();
            }

            if (!result.Item1)
            {
                // 🔴 SI YA ESTÁ BLOQUEADO EN LA BD
                if (mensajeServicio.Contains("bloqueado"))
                {
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    Input.Usuario = string.Empty;
                    Input.Contrasena = string.Empty;
                    return Page();
                }

                intentosFallidos++;

                if (intentosFallidos == 3)
                {
                    Input.MensajeError = "El usuario se bloqueó por fallar 3 intentos.";
                }
                else
                {
                    Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                }

                Input.Contrasena = string.Empty;
                return Page();
            }
            var usuario = result.Item3;

            // asegurar valores de avatar
            usuario.FotoPerfil = usuario.FotoPerfil ?? "";
            usuario.ColorAvatar = string.IsNullOrWhiteSpace(usuario.ColorAvatar) ? "#4285F4" : usuario.ColorAvatar;

            // si entra correctamente reiniciamos intentos
            intentosFallidos = 0;
            int rol = result.Item3.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            if (rol == 5)
            {
                return Redirect(adminUrl + "/Index");
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