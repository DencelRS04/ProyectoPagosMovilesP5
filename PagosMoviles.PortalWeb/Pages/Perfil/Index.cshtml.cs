using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Models.Perfil;
using PagosMoviles.PortalWeb.Services.Perfil;

namespace PagosMoviles.PortalWeb.Pages.Perfil
{
    public class IndexModel : PageModel
    {
        private readonly IPerfilService _perfilService;

        public IndexModel(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        [BindProperty]
        public PerfilViewModel Input { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuarioSesion = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuarioSesion == null)
                return Redirect("/");

            int usuarioId;
            if (!int.TryParse(usuarioSesion.UsuarioId, out usuarioId))
                return Redirect("/");

            var perfil = await _perfilService.ObtenerPerfilAsync(usuarioId);

            if (perfil == null)
            {
                Input = new PerfilViewModel
                {
                    UsuarioId = usuarioId,
                    NombreCompleto = usuarioSesion.UsuarioNombre ?? string.Empty,
                    Identificacion = string.Empty,
                    Telefono = string.Empty,
                    Email = string.Empty,
                    FotoPerfil = usuarioSesion.FotoPerfil ?? string.Empty,
                    ColorAvatar = string.IsNullOrWhiteSpace(usuarioSesion.ColorAvatar)
                        ? "#4285F4"
                        : usuarioSesion.ColorAvatar
                };

                Error = "No fue posible cargar la información completa del perfil.";
                return Page();
            }

            // 🔹 SOLUCIÓN PRINCIPAL
            var fotoPerfil = !string.IsNullOrWhiteSpace(perfil.FotoPerfil)
                ? perfil.FotoPerfil
                : usuarioSesion.FotoPerfil;

            var colorAvatar = !string.IsNullOrWhiteSpace(perfil.ColorAvatar)
                ? perfil.ColorAvatar
                : usuarioSesion.ColorAvatar;

            Input = new PerfilViewModel
            {
                UsuarioId = perfil.UsuarioId,
                NombreCompleto = perfil.NombreCompleto,
                Identificacion = perfil.Identificacion,
                Telefono = perfil.Telefono,
                Email = perfil.Email,
                FotoPerfil = fotoPerfil,
                ColorAvatar = string.IsNullOrWhiteSpace(colorAvatar)
                    ? "#4285F4"
                    : colorAvatar
            };

            // 🔹 Actualizar sesión
            usuarioSesion.FotoPerfil = Input.FotoPerfil;
            usuarioSesion.ColorAvatar = Input.ColorAvatar;
            usuarioSesion.UsuarioNombre = Input.NombreCompleto;

            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, usuarioSesion);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuarioSesion = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuarioSesion == null)
                return Redirect("/");

            if (Input == null)
            {
                Error = "Datos inválidos.";
                Input = new PerfilViewModel();
                return Page();
            }

            Input.NombreCompleto = (Input.NombreCompleto ?? "").Trim();
            Input.Telefono = (Input.Telefono ?? "").Trim();
            Input.ColorAvatar = string.IsNullOrWhiteSpace(Input.ColorAvatar)
                ? "#4285F4"
                : Input.ColorAvatar;

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

            if (perfilActualizado != null)
            {
                Input = new PerfilViewModel
                {
                    UsuarioId = perfilActualizado.UsuarioId,
                    NombreCompleto = perfilActualizado.NombreCompleto,
                    Identificacion = perfilActualizado.Identificacion,
                    Telefono = perfilActualizado.Telefono,
                    Email = perfilActualizado.Email,
                    FotoPerfil = perfilActualizado.FotoPerfil,
                    ColorAvatar = string.IsNullOrWhiteSpace(perfilActualizado.ColorAvatar)
                        ? "#4285F4"
                        : perfilActualizado.ColorAvatar
                };

                // 🔹 actualizar sesión
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