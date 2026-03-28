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

            // Obtiene el mensaje de sesión expirada guardado en sesión
            var mensajeExpirado = HttpContext.Session.GetString(SessionKeys.SessionExpiredMessage);

            if (!string.IsNullOrWhiteSpace(mensajeExpirado))
            {
                // Muestra el mensaje y luego lo elimina para que no se repita
                Mensaje = mensajeExpirado;
                HttpContext.Session.Remove(SessionKeys.SessionExpiredMessage);
            }
            else
            {
                // Si no hay mensaje, deja el texto vacío
                Mensaje = string.Empty;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Input.Usuario = (Input.Usuario ?? "").Trim();
            Input.Contrasena = (Input.Contrasena ?? "").Trim();

            // Código 400
            // Se usa cuando el usuario manda datos vacíos o incorrectos
            if (string.IsNullOrWhiteSpace(Input.Usuario) || string.IsNullOrWhiteSpace(Input.Contrasena))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                Input.MensajeError = "Usuario y/o contraseña incorrectos.";
                return Page();
            }
            else
            {
                // Obtiene el último usuario que intentó iniciar sesión
                var usuarioAnterior = HttpContext.Session.GetString("UsuarioIntento");

                if (usuarioAnterior != Input.Usuario)
                {
                    if (!string.IsNullOrEmpty(usuarioAnterior))
                    {
                        // Si cambió de usuario, elimina los intentos del usuario anterior
                        HttpContext.Session.Remove($"Intentos_{usuarioAnterior}");
                    }
                    else
                    {
                        // No había usuario anterior guardado
                    }

                    // Guarda el nuevo usuario actual en sesión
                    HttpContext.Session.SetString("UsuarioIntento", Input.Usuario);
                }
                else
                {
                    // Si es el mismo usuario, mantiene el conteo de intentos actual
                }

                var claveIntentos = $"Intentos_{Input.Usuario}";
                int intentos = HttpContext.Session.GetInt32(claveIntentos) ?? 0;

                // Código 423
                // Se usa cuando el usuario ya está bloqueado por demasiados intentos
                if (intentos >= MAX_INTENTOS)
                {
                    Response.StatusCode = StatusCodes.Status423Locked;
                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                    return Page();
                }
                else
                {
                    var result = await _authService.LoginAsync(Input.Usuario, Input.Contrasena);

                    // Código 500
                    // Se usa cuando el servicio devuelve null o falla de forma inesperada
                    if (result == null)
                    {
                        Response.StatusCode = StatusCodes.Status500InternalServerError;
                        Input.MensajeError = "Error en el servicio.";
                        return Page();
                    }
                    else
                    {
                        var mensajeServicio = (result.Item2 ?? "").ToLower();

                        // Código 404
                        // Se usa cuando el usuario no existe o no está registrado
                        if (mensajeServicio.Contains("no existe") || mensajeServicio.Contains("no registrado"))
                        {
                            Response.StatusCode = StatusCodes.Status404NotFound;
                            Input.MensajeError = "El usuario no está registrado.";
                            return Page();
                        }
                        else
                        {
                            // Cuando el login falla
                            if (!result.Item1)
                            {
                                // Código 423
                                // Se usa cuando el backend indica que el usuario está bloqueado
                                if (mensajeServicio.Contains("bloqueado"))
                                {
                                    Response.StatusCode = StatusCodes.Status423Locked;
                                    Input.MensajeError = "El usuario se encuentra bloqueado.";
                                    return Page();
                                }
                                // Código 500
                                // Se usa cuando el servicio reporta error interno
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
                                    // Código 400
                                    // Se usa cuando la contraseña o los datos son incorrectos
                                    Response.StatusCode = StatusCodes.Status400BadRequest;

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
                            }
                            else
                            {
                                // Login correcto
                                var usuario = result.Item3;

                                // Código 500
                                // Se usa si el login fue exitoso pero no vino el objeto usuario
                                if (usuario == null)
                                {
                                    Response.StatusCode = StatusCodes.Status500InternalServerError;
                                    Input.MensajeError = "No se pudo obtener la información del usuario.";
                                    return Page();
                                }
                                else
                                {
                                    // Limpia todos los intentos guardados en sesión
                                    var keys = new List<string>();

                                    foreach (var item in HttpContext.Session.Keys)
                                    {
                                        if (item.StartsWith("Intentos_"))
                                        {
                                            keys.Add(item);
                                        }
                                        else
                                        {
                                            // No es una llave de intentos
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

                                    // Si el usuario es administrador, lo redirige al portal admin
                                    if (rol == 5)
                                    {
                                        return Redirect(adminUrl + "/Index");
                                    }
                                    else
                                    {
                                        // Si es usuario normal, guarda la sesión y entra al portal
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
        }

        public IActionResult OnPostSetSessionExpired()
        {
            // Elimina el usuario actual de sesión
            HttpContext.Session.Remove("UsuarioId");

            // Guarda el mensaje para mostrarlo cuando vuelva a cargar el login
            HttpContext.Session.SetString("SessionExpiredMessage", "La sesión expiró por inactividad.");

            return new EmptyResult();
        }

        public IActionResult OnPostLogout()
        {
            // Limpia la sesión al cerrar sesión
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return Redirect("/");
        }
    }
}