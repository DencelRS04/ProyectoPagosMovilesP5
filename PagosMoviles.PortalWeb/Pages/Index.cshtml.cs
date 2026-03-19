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

        private const int ADMIN_ROL = 5;

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

            // 🔥 MENSAJE DE SESIÓN EXPIRADA
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

            // 🔥 MANEJO DE INTENTOS POR USUARIO (COMO ADMINWEB)
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
            var mensajeServicio = (result.Item2 ?? "").ToLower();

            // 🔴 SI NO EXISTE USUARIO
            if (result.Item3 == null)
            {
                Input.MensajeError = "El usuario no está registrado.";
                HttpContext.Session.Remove(claveIntentos);
                return Page();
            }

            // 🔴 LOGIN FALLIDO (CONTRASEÑA MALA)
            if (!result.Item1)
            {
                if (mensajeServicio.Contains("bloqueado"))
                {
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    return Page();
                }

                // 🔥 AQUÍ ESTÁ TU CAMBIO IMPORTANTE
                intentos++;
                HttpContext.Session.SetInt32(claveIntentos, intentos);

                if (intentos >= 3)
                {
                    Input.MensajeError = "El usuario se bloqueó por fallar 3 intentos.";
                }
                else
                {
                    // 👇 LO QUE PEDISTE
                    Input.MensajeError = "El usuario no está registrado.";
                }

                return Page();
            }

            var usuario = result.Item3;

            // 🔥 LOGIN CORRECTO → RESET COMPLETO
            HttpContext.Session.Remove(claveIntentos);
            HttpContext.Session.Remove("UsuarioIntento");

            usuario.FotoPerfil = usuario.FotoPerfil ?? "";
            usuario.ColorAvatar = string.IsNullOrWhiteSpace(usuario.ColorAvatar) ? "#4285F4" : usuario.ColorAvatar;

            int rol = usuario.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            // 🔥 ADMIN → REDIRIGE A ADMIN
            if (rol == ADMIN_ROL)
            {
                return Redirect(adminUrl + "/Index");
            }

            // 🔥 USUARIO NORMAL → PORTAL
            SessionHelper.LimpiarSesion(HttpContext.Session);
            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuario);

            return Redirect("/Home/Index");
        }

        // 🔥 ESTO TE FALTABA → POR ESO NO FUNCIONABA EL TIMEOUT
        public IActionResult OnPostSetSessionExpired()
        {
            HttpContext.Session.SetString(SessionKeys.SessionExpiredMessage, "La sesión expiró por inactividad.");
            return new EmptyResult();
        }

        public IActionResult OnPostLogout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/");
        }
    }
}