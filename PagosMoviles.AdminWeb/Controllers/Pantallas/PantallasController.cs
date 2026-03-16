using Microsoft.AspNetCore.Mvc;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.Shared.DTOs.Pantallas;

namespace PagosMoviles.AdminWeb.Controllers.Pantallas
{
    public class PantallasController : Controller
    {
        private readonly IPantallasService _service;

        public PantallasController(IPantallasService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var pantallas = await _service.ObtenerPantallas();
            return View(pantallas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PantallaCreateDto dto)
        {
            await _service.CrearPantalla(dto);
            return RedirectToAction("Index");
        }
    }
}

