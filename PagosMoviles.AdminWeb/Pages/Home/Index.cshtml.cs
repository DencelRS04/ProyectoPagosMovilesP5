using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Helpers;

namespace PagosMoviles.AdminWeb.Pages.Home
{
    public class IndexModel : PageModel
    {
        public string UsuarioNombre { get; set; } = "";

        public IActionResult OnGet()
        {
            if (TempData["SesionExpirada"] != null)
            {
                UsuarioNombre = "Administrador";
                return Page();
            }

            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);

            if (usuario == null)
            {
                return Redirect("/");
            }

            var ultimaActividad = SessionHelper.ObtenerUltimaActividad(HttpContext.Session);

            if (ultimaActividad.HasValue)
            {
                var diferencia = DateTime.UtcNow - ultimaActividad.Value;

                if (diferencia.TotalMinutes >= 5)
                {
                    SessionHelper.LimpiarSesion(HttpContext.Session);
                    TempData["SesionExpirada"] = "La sesión expiró por inactividad.";
                    UsuarioNombre = "Administrador";
                    return Page();
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