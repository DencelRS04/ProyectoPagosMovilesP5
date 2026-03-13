using Microsoft.AspNetCore.Mvc;
using PagosMoviles.PagosMovilesService.Auth;
using PagosMoviles.PagosMovilesService.Data;
using PagosMoviles.PagosMovilesService.Dtos;

namespace PagosMoviles.PagosMovilesService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly PagosMovilesRepository _repo;

    public AuthController(PagosMovilesRepository repo)
    {
        _repo = repo;
    }

    // SRV10
    [HttpPost("cancel-subscription")]
    [SrvAuthorize]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelarSuscripcionRequest req)
    {
        if (req == null ||
            string.IsNullOrWhiteSpace(req.NumeroCuenta) ||
            string.IsNullOrWhiteSpace(req.Identificacion) ||
            string.IsNullOrWhiteSpace(req.Telefono))
        {
            return BadRequest(new { codigo = -1, descripcion = "Datos incorrectos" });
        }

        var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);
        if (afiliacion == null)
        {
            return NotFound(new { codigo = -1, descripcion = "Teléfono no se encuentra afiliado" });
        }

        var rows = await _repo.CancelarSuscripcionAsync(req.Telefono, req.Identificacion, req.NumeroCuenta);
        if (rows == 0)
        {
            return BadRequest(new { codigo = -1, descripcion = "Datos incorrectos" });
        }

        await _repo.RegistrarBitacoraAsync(req.Identificacion, "Cancelación de suscripción");

        return Ok(new { codigo = 0, descripcion = "Desinscripción realizada" });
    }
}