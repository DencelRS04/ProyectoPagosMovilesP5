using Microsoft.AspNetCore.Mvc;
using PagosMoviles.PortalWeb.Helpers;
using PagosMoviles.PortalWeb.Models.Auth;
using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.Shared.Constants;

namespace PagosMoviles.PortalWeb.Controllers.Auth
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? mensaje = null)
        {
            var mensajeExpirada = HttpContext.Session.GetString(SessionKeys.SessionExpiredMessage);
            if (!string.IsNullOrWhiteSpace(mensajeExpirada))
            {
                ViewBag.Mensaje = mensajeExpirada;
                HttpContext.Session.Remove(SessionKeys.SessionExpiredMessage);
            }
            else
            {
                ViewBag.Mensaje = mensaje;
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(model.Usuario, model.Contrasena);

            if (!result.Exito || result.Usuario is null)
            {
                model.MensajeError = result.Mensaje;
                return View(model);
            }

            SessionHelper.GuardarUsuarioSesion(HttpContext.Session, result.Usuario);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            SessionHelper.LimpiarSesion(HttpContext.Session);
            return RedirectToAction("Login", "Auth");
        }
    }
}
