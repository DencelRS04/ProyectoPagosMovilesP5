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

        public IndexModel(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        public string Mensaje { get; set; } = string.Empty;

        private const int MAX_INTENTOS = 3;

        public void OnGet()
        {
            Input = new LoginInputModel();

            // 🔥 MENSAJE SESIÓN EXPIRADA
            var mensajeExpirado = HttpContext.Session.GetString(SessionKeys.SessionExpiredMessage);

            if (!string.IsNullOrWhiteSpace(mensajeExpirado))
            {
                Mensaje = mensajeExpirado;
                HttpContext.Session.Remove(SessionKeys.SessionExpiredMessage);
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

            // 🔥 MANEJO DE INTENTOS POR USUARIO
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

            if (intentos >= MAX_INTENTOS)
            {
                Input.MensajeError = "El usuario se encuentra bloqueado.";
                return Page();
            }

            var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);
            var mensajeServicio = (result?.Item2 ?? "").ToLower();

            if (result == null)
            {
                Input.MensajeError = "Error en el servicio.";
                return Page();
            }

            // 🔴 USUARIO NO EXISTE
            if (mensajeServicio.Contains("no existe") || mensajeServicio.Contains("no registrado"))
            {
                Input.MensajeError = "El usuario no está registrado.";
                return Page();
            }

            // 🔴 LOGIN FALLIDO
            if (!result.Item1)
            {
                if (mensajeServicio.Contains("bloqueado"))
                {
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    return Page();
                }

                intentos++;
                HttpContext.Session.SetInt32(claveIntentos, intentos);

                if (intentos >= MAX_INTENTOS)
                {
                    Input.MensajeError = "El usuario se bloqueó por fallar 3 intentos.";
                }
                else
                {
                    Input.MensajeError = $"Usuario y/o contraseña incorrectos. Intento {intentos} de 3.";
                }

                return Page();
            }

            // ✅ LOGIN CORRECTO
            var usuario = result.Item3;

            // 🔥 AQUÍ ESTÁ TU CAMBIO (RESET GLOBAL)
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

            int rol = usuario.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            // 🔥 ADMIN → REDIRECCIÓN
            if (rol == 5)
            {
                return Redirect(adminUrl + "/Index");
            }

            // 🔥 USUARIO NORMAL
            SessionHelper.LimpiarSesion(HttpContext.Session);
            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuario);

            return Redirect("/Home/Index");
        }

        // 🔥 SESSION TIMEOUT
        public IActionResult OnPostSetSessionExpired()
        {
            HttpContext.Session.Remove("UsuarioId");

            HttpContext.Session.SetString("SessionExpiredMessage", "La sesión expiró por inactividad.");

            return new EmptyResult();
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/");
        }
    }
}