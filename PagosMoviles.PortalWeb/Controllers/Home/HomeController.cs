using Microsoft.AspNetCore.Mvc;
using PagosMoviles.PortalWeb.Filters;
using PagosMoviles.PortalWeb.Helpers;
namespace PagosMoviles.PortalWeb.Controllers.Home
{

    [PortalAuthorize]
    [ServiceFilter(typeof(SessionExpirationFilter))]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);
            ViewBag.UsuarioNombre = usuario?.UsuarioNombre ?? "Usuario";
            return View();
        }
    }
}
