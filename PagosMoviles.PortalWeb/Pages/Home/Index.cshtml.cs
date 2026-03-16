using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.PortalWeb.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string UsuarioNombre { get; set; } = "";

        public IActionResult OnGet()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuario == null)
            {
                return Redirect("/");
            }

            int rol = usuario.RolId;

            var adminUrl = _configuration["PortalSettings:AdminUrl"] ?? "https://localhost:7150";

            if (rol == 5)
            {
                SessionHelper.LimpiarSesion(HttpContext.Session);
                return Redirect(adminUrl + "/Index");
            }

            var ultimaActividad = SessionHelper.ObtenerUltimaActividad(HttpContext.Session);

            if (ultimaActividad.HasValue)
            {
                var diferencia = DateTime.UtcNow - ultimaActividad.Value;

                if (diferencia.TotalMinutes >= 5)
                {
                    SessionHelper.LimpiarSesion(HttpContext.Session);

                    HttpContext.Session.SetString(
                        SessionKeys.SessionExpiredMessage,
                        "La sesión ha expirado por inactividad."
                    );

                    return Redirect("/");
                }
            }

            SessionHelper.ActualizarActividad(HttpContext.Session);

            UsuarioNombre = string.IsNullOrWhiteSpace(usuario.UsuarioNombre)
                ? "Cliente"
                : usuario.UsuarioNombre;

            return Page();
        }
    }
}