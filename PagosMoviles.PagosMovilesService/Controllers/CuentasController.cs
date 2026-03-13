using Microsoft.AspNetCore.Mvc;
using PagosMoviles.PagosMovilesService.Auth;
using PagosMoviles.PagosMovilesService.Data;
using PagosMoviles.PagosMovilesService.Dtos;
using PagosMoviles.PagosMovilesService.Services;
using System.Data.SqlClient;
using Dapper;

namespace PagosMoviles.PagosMovilesService.Controllers
{
    [ApiController]
    [Route("accounts")]
    public class CuentasController : ControllerBase
    {
        private readonly PagosMovilesRepository _repo;
        private readonly CoreSrvClient _core;

        public CuentasController(PagosMovilesRepository repo, CoreSrvClient core)
        {
            _repo = repo;
            _core = core;
        }

        // SRV13 - Consultar saldo (como lo tenías; si ya lo conectaste a SRV15 en otro método, déjalo igual)
        [HttpPost("balance")]
        [SrvAuthorize]
        public async Task<IActionResult> ConsultarSaldo([FromBody] ConsultarSaldoRequest req)
        {
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Telefono) ||
                string.IsNullOrWhiteSpace(req.Identificacion))
            {
                return BadRequest(new { codigo = -1, descripcion = "Debe enviar los datos completos y válidos" });
            }

            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacion == null || afiliacion.Estado == false || afiliacion.Identificacion != req.Identificacion)
            {
                return BadRequest(new { codigo = -1, descripcion = "Cliente no asociado a pagos móviles." });
            }

            // Si tu saldo hoy te funciona por BD o por CoreSrv (SRV15) no lo toco aquí.
            // Dejé tu repo como antes:
            var saldo = await _repo.GetSaldoAsync(afiliacion.Identificacion, afiliacion.NumeroCuenta);

            if (saldo == null)
            {
                return BadRequest(new { codigo = -1, descripcion = "Cliente no asociado a pagos móviles." });
            }

            return Ok(new { codigo = 0, saldo });
        }

        // SRV11 - Últimos movimientos (AHORA CON SRV16 del Core)
        [HttpPost("transactions")]
        [SrvAuthorize]
        public async Task<IActionResult> UltimosMovimientos([FromBody] ConsultarMovimientosRequest req)
        {
            if (req == null ||
                string.IsNullOrWhiteSpace(req.Telefono) ||
                string.IsNullOrWhiteSpace(req.Identificacion))
            {
                return BadRequest(new { codigo = -1, descripcion = "Debe enviar los datos completos y válidos" });
            }

            // 1) validar afiliación (BD PagosMoviles)
            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacion == null || afiliacion.Estado == false || afiliacion.Identificacion != req.Identificacion)
            {
                return BadRequest(new { codigo = -1, descripcion = "Cliente no asociado a pagos móviles." });
            }

            // 2) tomar token (por si luego usan auth real; con DEV funciona igual)
            var authHeader = Request.Headers.Authorization.ToString();
            string? token = null;
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
                token = authHeader["Bearer ".Length..].Trim();

            // 3) llamar SRV16 en Core: POST /core/movimientos/ultimos
            var coreResp = await _core.UltimosMovimientosAsync(
                afiliacion.Identificacion,
                afiliacion.NumeroCuenta,
                token
            );

            if (coreResp == null || coreResp.Success == false || coreResp.Data == null)
            {
                var msg = coreResp?.Message;
                if (string.IsNullOrWhiteSpace(msg))
                    msg = "No fue posible consultar movimientos en Core Bancario.";

                return NotFound(new { codigo = -1, descripcion = msg });
            }

            // 4) mapear a tu DTO
            var movimientos = coreResp.Data.Select(x => new MovimientoDto
            {
                TipoMovimiento = x.TipoMovimiento,
                Monto = x.Monto,
                Fecha = x.Fecha
            });

            return Ok(new { codigo = 0, movimientos });
        }
    }
}