using Microsoft.AspNetCore.Mvc;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.Shared.DTOs.Saldo;

namespace PagosMoviles.PortalWeb.Controllers.Saldo
{
    [Route("saldo")]
    public class SaldoController : Controller
    {
        private readonly ISaldoService _saldoService;

        public SaldoController(ISaldoService saldoService)
        {
            _saldoService = saldoService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("consultar")]
        public async Task<IActionResult> Consultar(string telefono, string identificacion)
        {
            var resultado = await _saldoService.ConsultarSaldo(telefono, identificacion);

            return Json(resultado);
        }
    }
}