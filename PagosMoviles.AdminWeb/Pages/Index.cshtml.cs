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
            else
            {
                // No hay mensaje de sesión expirada
                Mensaje = string.Empty;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Input.Usuario = (Input.Usuario ?? "").Trim();
            Input.Contrasena = (Input.Contrasena ?? "").Trim();

            // 400: BadRequest  Datos vacíos o inválidos enviados por el usuario
            if (string.IsNullOrWhiteSpace(Input.Usuario) || string.IsNullOrWhiteSpace(Input.Contrasena))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                return Page();
            }
            else
            {
                var usuarioAnterior = HttpContext.Session.GetString("UsuarioIntento");

                if (usuarioAnterior != Input.Usuario)
                {
                    if (!string.IsNullOrEmpty(usuarioAnterior))
                    {
                        HttpContext.Session.Remove($"Intentos_{usuarioAnterior}");
                    }
                    else
                    {
                        // No había usuario anterior
                    }

                    HttpContext.Session.SetString("UsuarioIntento", Input.Usuario);
                }
                else
                {
                    // Mismo usuario, mantiene intentos
                }

                var claveIntentos = $"Intentos_{Input.Usuario}";
                int intentos = HttpContext.Session.GetInt32(claveIntentos) ?? 0;

                // 423: Locked  Usuario bloqueado por demasiados intentos fallidos
                if (intentos >= 3)
                {
                    Response.StatusCode = StatusCodes.Status423Locked;
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    return Page();
                }
                else
                {
                    var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

                    // 500: InternalServerError  El servicio devolvió null (error inesperado)
                    if (result == null)
                    {
                        Response.StatusCode = StatusCodes.Status500InternalServerError;
                        Input.MensajeError = "Ocurrió un error interno al procesar el inicio de sesión.";
                        return Page();
                    }
                    else
                    {
                        if (!result.Item1)
                        {
                            var mensajeServicio = (result.Item2 ?? "").ToLower();

                            // 404: NotFound  Usuario no existe en el sistema
                            if (mensajeServicio.Contains("no existente") ||
                                mensajeServicio.Contains("no existe") ||
                                mensajeServicio.Contains("no encontrado"))
                            {
                                Response.StatusCode = StatusCodes.Status404NotFound;
                                Input.MensajeError = "Usuario no existente.";
                                return Page();
                            }
                            // 423: Locked  Usuario bloqueado desde backend
                            else if (mensajeServicio.Contains("bloqueado"))
                            {
                                Response.StatusCode = StatusCodes.Status423Locked;
                                Input.MensajeError = "El usuario se encuentra bloqueado.";
                                return Page();
                            }
                            // 500: InternalServerError  Error interno del backend
                            else if (mensajeServicio.Contains("error interno") ||
                                     mensajeServicio.Contains("exception") ||
                                     mensajeServicio.Contains("servidor"))
                            {
                                Response.StatusCode = StatusCodes.Status500InternalServerError;
                                Input.MensajeError = "Ocurrió un error interno del servidor.";
                                return Page();
                            }
                            else
                            {
                                // 400: BadRequest  Credenciales incorrectas
                                Response.StatusCode = StatusCodes.Status400BadRequest;

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
                        }
                        else
                        {
                            var usuario = result.Item3;

                            // 500: InternalServerError no se obtuvo el usuario correctamente
                            if (usuario == null)
                            {
                                Response.StatusCode = StatusCodes.Status500InternalServerError;
                                Input.MensajeError = "No se pudo obtener la información del usuario.";
                                return Page();
                            }
                            else
                            {
                                var keys = new List<string>();

                                foreach (var item in HttpContext.Session.Keys)
                                {
                                    if (item.StartsWith("Intentos_"))
                                    {
                                        keys.Add(item);
                                    }
                                    else
                                    {
                                        // No es intento
                                    }
                                }

                                foreach (var key in keys)
                                {
                                    HttpContext.Session.Remove(key);
                                }

                                HttpContext.Session.Remove("UsuarioIntento");

                                usuario.FotoPerfil = usuario.FotoPerfil ?? "";
                                usuario.ColorAvatar = string.IsNullOrWhiteSpace(usuario.ColorAvatar) ? "#4285F4" : usuario.ColorAvatar;

                                // 403: Forbidden  Usuario sin permisos de administrador
                                if (usuario.RolId != ADMIN_ROL)
                                {
                                    Response.StatusCode = StatusCodes.Status403Forbidden;
                                    Input.MensajeError = "No tiene permisos para acceder al portal administrativo.";
                                    return Page();
                                }
                                else
                                {
                                    SessionHelper.LimpiarSesion(HttpContext.Session);
                                    SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuario);

                                    return Redirect("/Home/Index");
                                }
                            }
                        }
                    }
                }
            }
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