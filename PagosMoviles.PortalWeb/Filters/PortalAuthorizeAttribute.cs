using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.PortalWeb.Helpers;
namespace PagosMoviles.PortalWeb.Filters
{
    public class PortalAuthorizeAttribute : ActionFilterAttribute
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

            base.OnActionExecuting(context);
        }
    }
}
