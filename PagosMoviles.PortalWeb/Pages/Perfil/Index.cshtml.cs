using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Services.Perfil;
using PagosMoviles.PortalWeb.Models.Perfil;

namespace PagosMoviles.PortalWeb.Pages.Perfil
{
    public class IndexModel : PageModel
    {
        private readonly IPerfilService _perfilService;
        private readonly IConfiguration _configuration;

        public IndexModel(IPerfilService perfilService, IConfiguration configuration)
        {
            _perfilService = perfilService;
            _configuration = configuration;
        }

        [BindProperty]
        public PerfilViewModel Input { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuarioSesion = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuarioSesion == null)
                return Redirect("/Auth/Login");

            int rol = usuarioSesion.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            // ADMIN = 5
            if (rol == 5)
                return Redirect(adminUrl + "/Home/Index");

            int usuarioId;
            if (!int.TryParse(usuarioSesion.UsuarioId, out usuarioId))
                return Redirect("/Auth/Login");

            var perfil = await _perfilService.ObtenerPerfilAsync(usuarioId);

            if (perfil == null || !perfil.Exito)
            {
                Input = new PerfilViewModel
                {
                    UsuarioId = usuarioId,
                    NombreCompleto = usuarioSesion.UsuarioNombre ?? string.Empty,
                    Identificacion = string.Empty,
                    Telefono = string.Empty,
                    Email = string.Empty,
                    FotoPerfil = usuarioSesion.FotoPerfil ?? string.Empty,
                    ColorAvatar = string.IsNullOrWhiteSpace(usuarioSesion.ColorAvatar) ? "#4285F4" : usuarioSesion.ColorAvatar
                };

                Error = "No fue posible cargar la información completa del perfil." +
                        (perfil != null && !string.IsNullOrWhiteSpace(perfil.ErrorDetalle)
                            ? " " + perfil.ErrorDetalle
                            : string.Empty);

                return Page();
            }

            Input = new PerfilViewModel
            {
                UsuarioId = perfil.UsuarioId,
                NombreCompleto = perfil.NombreCompleto ?? string.Empty,
                Identificacion = perfil.Identificacion ?? string.Empty,
                Telefono = perfil.Telefono ?? string.Empty,
                Email = perfil.Email ?? string.Empty,
                FotoPerfil = perfil.FotoPerfil ?? string.Empty,
                ColorAvatar = string.IsNullOrWhiteSpace(perfil.ColorAvatar) ? "#4285F4" : perfil.ColorAvatar
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuarioSesion = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuarioSesion == null)
                return Redirect("/Auth/Login");

            int rol = usuarioSesion.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            if (rol == 5)
                return Redirect(adminUrl + "/Home/Index");

            if (Input == null)
            {
                Error = "Datos inválidos.";
                Input = new PerfilViewModel();
                return Page();
            }

            Input.NombreCompleto = (Input.NombreCompleto ?? "").Trim();
            Input.Telefono = (Input.Telefono ?? "").Trim();
            Input.ColorAvatar = string.IsNullOrWhiteSpace(Input.ColorAvatar) ? "#4285F4" : Input.ColorAvatar;

            if (string.IsNullOrWhiteSpace(Input.NombreCompleto))
            {
                Error = "El nombre es obligatorio.";
                return Page();
            }

            var ok = await _perfilService.ActualizarPerfilAsync(Input);

            if (!ok)
            {
                Error = "No fue posible guardar los cambios.";
                return Page();
            }

            var perfilActualizado = await _perfilService.ObtenerPerfilAsync(Input.UsuarioId);

            if (perfilActualizado != null && perfilActualizado.Exito)
            {
                Input = new PerfilViewModel
                {
                    UsuarioId = perfilActualizado.UsuarioId,
                    NombreCompleto = perfilActualizado.NombreCompleto,
                    Identificacion = perfilActualizado.Identificacion,
                    Telefono = perfilActualizado.Telefono,
                    Email = perfilActualizado.Email,
                    FotoPerfil = perfilActualizado.FotoPerfil,
                    ColorAvatar = string.IsNullOrWhiteSpace(perfilActualizado.ColorAvatar) ? "#4285F4" : perfilActualizado.ColorAvatar
                };

                usuarioSesion.UsuarioNombre = Input.NombreCompleto;
                usuarioSesion.FotoPerfil = Input.FotoPerfil;
                usuarioSesion.ColorAvatar = Input.ColorAvatar;

                SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuarioSesion);
            }

            Mensaje = "Perfil actualizado correctamente.";
            return Page();
        }
    }
}