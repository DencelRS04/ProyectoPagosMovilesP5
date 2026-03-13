using Microsoft.AspNetCore.Mvc;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Security;
using PagosMoviles.CoreBancarioService.Services;

namespace PagosMoviles.CoreBancarioService.Controllers
{
    [ApiController]
    [Route("core/movimientos")]
    public class MovimientoController : ControllerBase
    {
        private readonly CoreTransaccionService _service;
        private readonly CoreGatewayBitacoraClient _bitacora;

        public MovimientoController(CoreTransaccionService service, CoreGatewayBitacoraClient bitacora)
        {
            _service = service;
            _bitacora = bitacora;
        }

        // =========================
        // SRV16 - Últimos 5 movimientos
        // POST /core/movimientos/ultimos
        // =========================
        [HttpPost("ultimos")]
        public async Task<IActionResult> Ultimos([FromBody] ConsultaCuentaDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV16 Ultimos: datos incorrectos",
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

            var resp = _service.UltimosMovimientos(dto.Identificacion, dto.NumeroCuenta);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV16 Ultimos: no encontrado",
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
                "SRV16 Ultimos OK",
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

        // =========================
        // SRV14 - Aplicar transacción
        // POST /core/movimientos/aplicar
        // =========================
        [HttpPost("aplicar")]
        public async Task<IActionResult> Aplicar([FromBody] AplicarTransaccionDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV14 Aplicar: datos incorrectos",
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

            var resp = _service.AplicarTransaccion(dto);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV14 Aplicar: conflicto",
                    null,
                    dto,
                    token
                );

                return Conflict(new
                {
                    codigo = 409,
                    descripcion = resp.Message,
                    datos = (object?)null
                });
            }

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SRV14 Aplicar OK",
                null,
                dto,
                token
            );

            return Ok(new
            {
                codigo = 200,
                descripcion = "Transacción aplicada",
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