using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.AdminWeb.Pages.Home
{
    public class IndexModel : PageModel
    {
        public string UsuarioNombre { get; set; }

        public IActionResult OnGet()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuario == null)
            {
                return Redirect("/Auth/Login");
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
                        "La sesión expiró por inactividad."
                    );

                    return Redirect("/Auth/Login");
                }
            }

            SessionHelper.ActualizarActividad(HttpContext.Session);

            UsuarioNombre = string.IsNullOrWhiteSpace(usuario.UsuarioNombre)
                ? "Administrador"
                : usuario.UsuarioNombre;

            return Page();
        }
    }
}