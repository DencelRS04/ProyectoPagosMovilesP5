using Microsoft.AspNetCore.Mvc;
using PagosMoviles.AdminWeb.Filters;
using PagosMoviles.AdminWeb.Helpers;


namespace PagosMoviles.AdminWeb.Controllers.Home
{
    [AdminAuthorize]
    [ServiceFilter(typeof(SessionExpirationFilter))]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(HttpContext.Session);
            ViewBag.UsuarioNombre = usuario?.UsuarioNombre ?? "Administrador";
            return View();
        }
    }
}
