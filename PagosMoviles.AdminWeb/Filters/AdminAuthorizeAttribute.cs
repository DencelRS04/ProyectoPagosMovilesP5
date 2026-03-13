using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.AdminWeb.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var usuario = SessionHelper.ObtenerUsuarioSesion(session);

            if (usuario is null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new
                {
                    area = "",
                    mensaje = "Por favor inicie sesión para utilizar el sistema."
                });
                return;
            }

            if (!string.Equals(usuario.Rol, Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new
                {
                    area = "",
                    mensaje = "No autorizado para ingresar al sitio administrativo."
                });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
