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

            var mensajeExpirado = HttpContext.Session.GetString("SessionExpiredMessage");

            if (!string.IsNullOrWhiteSpace(mensajeExpirado))
            {
                Mensaje = mensajeExpirado;
                HttpContext.Session.Remove("SessionExpiredMessage");
            }
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

            // 🔥 DETECTAR CAMBIO DE USUARIO
            var usuarioAnterior = HttpContext.Session.GetString("UsuarioIntento");

            if (usuarioAnterior != Input.Usuario)
            {
                if (!string.IsNullOrEmpty(usuarioAnterior))
                {
                    HttpContext.Session.Remove($"Intentos_{usuarioAnterior}");
                }

                HttpContext.Session.SetString("UsuarioIntento", Input.Usuario);
            }

            var claveIntentos = $"Intentos_{Input.Usuario}";
            int intentos = HttpContext.Session.GetInt32(claveIntentos) ?? 0;

            if (intentos >= 3)
            {
                Input.MensajeError = "El usuario se encuentra bloqueado.";
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

            if (!result.Item1)
            {
                var mensajeServicio = (result.Item2 ?? "").ToLower();

                // 🔥 VALIDACIÓN: USUARIO NO EXISTE
                if (mensajeServicio.Contains("no existente") ||
                    mensajeServicio.Contains("no existe") ||
                    mensajeServicio.Contains("no encontrado"))
                {
                    Input.MensajeError = "Usuario no existente.";
                    return Page();
                }

                if (mensajeServicio.Contains("bloqueado"))
                {
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    return Page();
                }

                intentos++;
                HttpContext.Session.SetInt32(claveIntentos, intentos);

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

            var usuario = result.Item3;

            // 🔥 RESET GLOBAL DE INTENTOS
            var keys = new List<string>();

            foreach (var item in HttpContext.Session.Keys)
            {
                if (item.StartsWith("Intentos_"))
                {
                    keys.Add(item);
                }
            }

            foreach (var key in keys)
            {
                HttpContext.Session.Remove(key);
            }

            HttpContext.Session.Remove("UsuarioIntento");

            usuario.FotoPerfil = usuario.FotoPerfil ?? "";
            usuario.ColorAvatar = string.IsNullOrWhiteSpace(usuario.ColorAvatar) ? "#4285F4" : usuario.ColorAvatar;

            if (usuario.RolId != ADMIN_ROL)
            {
                Input.MensajeError = "No tiene permisos para acceder al portal administrativo.";
                return Page();
            }

            SessionHelper.LimpiarSesion(HttpContext.Session);
            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuario);

            return Redirect("/Home/Index");
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            HttpContext.Session.Clear();

            return RedirectToPage("/Index");
        }

        public IActionResult OnPostSetSessionExpired()
        {
            HttpContext.Session.Remove("UsuarioId");
            HttpContext.Session.SetString("SessionExpiredMessage", "La sesión expiró por inactividad.");

            return new EmptyResult();
        }
    }
}