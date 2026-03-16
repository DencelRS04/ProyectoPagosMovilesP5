using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.AdminWeb.Helpers;

namespace PagosMoviles.AdminWeb.Filters
{
    public class SessionExpirationFilter : ActionFilterAttribute
    {
        private readonly int _timeoutMinutes;

        public SessionExpirationFilter(IConfiguration configuration)
        {
            _timeoutMinutes = configuration.GetValue<int>("SessionSettings:TimeoutMinutes", 5);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var usuario = SessionHelper.ObtenerUsuarioSesion(session);

            if (usuario == null)
            {
                base.OnActionExecuting(context);
                return;
            }

            var ultimaActividad = SessionHelper.ObtenerUltimaActividad(session);

            if (ultimaActividad.HasValue)
            {
                var minutosInactivo = DateTime.UtcNow - ultimaActividad.Value;

                if (minutosInactivo.TotalMinutes >= _timeoutMinutes)
                {
                    SessionHelper.LimpiarSesion(session);
                    context.Result = new RedirectResult("/Auth/Login?mensaje=La%20sesi%C3%B3n%20ha%20expirado%20por%20inactividad.");
                    return;
                }
            }

            SessionHelper.ActualizarActividad(session);
            base.OnActionExecuting(context);
        }
    }
}