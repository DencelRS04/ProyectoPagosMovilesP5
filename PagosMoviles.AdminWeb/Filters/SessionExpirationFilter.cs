using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.Shared.Constants;

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
                    TempDataExtensions.SetTempMessage(context.HttpContext, SessionKeys.SessionExpiredMessage,
                        "La sesión ha expirado por inactividad.");

                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
            }

            SessionHelper.ActualizarActividad(session);
            base.OnActionExecuting(context);
        }
    }

    internal static class TempDataExtensions
    {
        public static void SetTempMessage(HttpContext httpContext, string key, string value)
        {
            httpContext.Session.SetString(key, value);
        }
    }
}
