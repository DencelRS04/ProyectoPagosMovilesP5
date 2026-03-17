using Microsoft.AspNetCore.Mvc;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.Shared.DTOs.Roles;

namespace PagosMoviles.AdminWeb.Controllers.Roles
{
    public class RolesController : Controller
    {
        private readonly IRolesService _service;

        public RolesController(IRolesService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _service.ObtenerRoles();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RolCreateDto dto)
        {
            await _service.CrearRol(dto);
            return RedirectToAction("Index");
        }
    }
}