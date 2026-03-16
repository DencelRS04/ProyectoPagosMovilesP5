using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.PortalWeb.Filters
{
    /*public class SessionExpirationFilter : ActionFilterAttribute
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

            if (usuario is null)
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
                    context.HttpContext.Session.SetString(
                        SessionKeys.SessionExpiredMessage,
                        "La sesión ha expirado por inactividad.");

                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
            }

            SessionHelper.ActualizarActividad(session);
            base.OnActionExecuting(context);
        }
    }*/
}
