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

            if (usuario == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (usuario.RolId != Roles.Admin)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}