using Microsoft.AspNetCore.Mvc;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Services;
using PagosMoviles.CoreBancarioService.Security;

namespace PagosMoviles.CoreBancarioService.Controllers
{
    [ApiController]
    [Route("core")]
    public class CoreBalanceController : ControllerBase
    {
        private readonly CoreCuentaService _service;
        private readonly CoreGatewayBitacoraClient _bitacora;

        public CoreBalanceController(CoreCuentaService service, CoreGatewayBitacoraClient bitacora)
        {
            _service = service;
            _bitacora = bitacora;
        }

        // SRV15
        // GET /core/balance?identificacion=...&numeroCuenta=...
        [HttpGet("balance")]
        public async Task<IActionResult> Balance([FromQuery] ConsultaCuentaDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV15 Balance: datos incorrectos",
                    null,
                    dto,
                    token
                );

                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Datos incorrectos",
                    datos = (object?)null
                });
            }

            var resp = _service.ConsultarSaldo(dto.Identificacion, dto.NumeroCuenta);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV15 Balance: cuenta no encontrada",
                    null,
                    dto,
                    token
                );

                return NotFound(new
                {
                    codigo = 404,
                    descripcion = resp.Message,
                    datos = (object?)null
                });
            }

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SRV15 Balance OK",
                null,
                dto,
                token
            );

            return Ok(new
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = resp.Data
            });
        }

        private string? GetBearerToken()
        {
            var auth = Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(auth) &&
                auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return auth.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}