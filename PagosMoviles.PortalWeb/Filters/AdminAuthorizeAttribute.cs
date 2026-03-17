using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.PortalWeb.Helpers;
using System;

namespace PagosMoviles.PortalWeb.Filters
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

            // ADMIN = 5
            if (usuario.RolId != 5)
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