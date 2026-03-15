using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagosMoviles.API.Data;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Services;
using PagosMoviles.PagosMovilesService.Dtos;
using System.Text.RegularExpressions;

namespace PagosMoviles.API.Controllers;

[ApiController]
[Route("accounts")]
[Tags("Cuentas")]
[Consumes("application/json")]
[Produces("application/json")]
public class CuentasController : ControllerBase
{
    private readonly PagosMovilesRepository _repo;
    private readonly CoreSrvClient _core;

    // Teléfono CR: 8 dígitos, inicia 2/4/5/6/7/8
    private static readonly Regex TelefonoCR = new(@"^(?:2|4|5|6|7|8)\d{7}$", RegexOptions.Compiled);

    public CuentasController(PagosMovilesRepository repo, CoreSrvClient core)
    {
        _repo = repo;
        _core = core;
    }

    // =========================================================
    // SRV13 - Consultar saldo
    // POST /accounts/balance
    // Body: { telefono, identificacion }
    // =========================================================
    [HttpPost("balance")]
    public async Task<IActionResult> ConsultarSaldo([FromBody] ConsultarSaldoRequest req)
    {
        var usuarioBitacora = req?.Identificacion ?? "SYSTEM";

        try
        {
            // =========================
            // Validaciones request
            // =========================
            if (req is null)
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV13 ConsultarSaldo: body nulo");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Debe enviar el body del request.",
                    datos = null
                });
            }

            if (string.IsNullOrWhiteSpace(req.Telefono))
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, "SRV13 ConsultarSaldo: teléfono requerido");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El teléfono es requerido.",
                    datos = null
                });
            }

            if (req.Telefono != req.Telefono.Trim())
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, "SRV13 ConsultarSaldo: teléfono con espacios");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El teléfono no puede tener espacios al inicio o al final.",
                    datos = null
                });
            }

            if (!TelefonoCR.IsMatch(req.Telefono))
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, $"SRV13 ConsultarSaldo: teléfono inválido ({req.Telefono})");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Teléfono inválido. Debe ser de Costa Rica: 8 dígitos e iniciar con 2,4,5,6,7 u 8.",
                    datos = null
                });
            }

            if (string.IsNullOrWhiteSpace(req.Identificacion))
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV13 ConsultarSaldo: identificación requerida");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "La identificación es requerida.",
                    datos = null
                });
            }

            if (req.Identificacion != req.Identificacion.Trim())
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV13 ConsultarSaldo: identificación con espacios");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "La identificación no puede tener espacios al inicio o al final.",
                    datos = null
                });
            }

            // =========================
            // Buscar afiliación por teléfono
            // =========================
            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacion is null)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV13: teléfono no afiliado ({req.Telefono})");
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Teléfono no se encuentra afiliado.",
                    datos = null
                });
            }

            // =========================
            // Validar que la identificación coincida
            // =========================
            if (!string.Equals(afiliacion.Identificacion, req.Identificacion, StringComparison.OrdinalIgnoreCase))
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV13: identificación no coincide para {req.Telefono}");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Identificación no coincide con el teléfono afiliado.",
                    datos = null
                });
            }

            // =========================
            // Validar estado
            // =========================
            if (!afiliacion.Estado)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV13: teléfono desinscrito ({req.Telefono})");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El teléfono se encuentra desinscrito.",
                    datos = afiliacion
                });
            }

            // =========================
            // Consultar saldo
            // =========================
            var saldo = await _repo.GetSaldoAsync(afiliacion.Identificacion, afiliacion.NumeroCuenta);

            if (saldo is null)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV13: saldo no encontrado. Cuenta={afiliacion.NumeroCuenta}");
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "No se encontró saldo para la cuenta asociada.",
                    datos = afiliacion
                });
            }

            await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV13 OK Tel={req.Telefono} Cuenta={afiliacion.NumeroCuenta}");

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = new
                {
                    afiliacion,
                    saldo
                }
            });
        }
        catch (Exception ex)
        {
            await SafeBitacora(usuarioBitacora, $"SRV13 ERROR: {ex.Message}");
            return StatusCode(500, new ApiResponse
            {
                codigo = 500,
                descripcion = "Error interno del servidor",
                datos = null
            });
        }
    }

    // =========================================================
    // SRV11 - Últimos 5 movimientos (Core SRV16)
    // POST /accounts/transactions
    // Body: { telefono, identificacion }
    // =========================================================
    [HttpPost("transactions")]
    public async Task<IActionResult> UltimosMovimientos([FromBody] ConsultarMovimientosRequest req)
    {
        var usuarioBitacora = req?.Identificacion ?? "SYSTEM";

        try
        {
            // Validaciones request
            if (req is null)
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV11 UltimosMovimientos: body nulo");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "Debe enviar el body del request.", datos = null });
            }

            if (string.IsNullOrWhiteSpace(req.Telefono))
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, "SRV11 UltimosMovimientos: teléfono requerido");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "El teléfono es requerido.", datos = null });
            }

            if (req.Telefono != req.Telefono.Trim())
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, "SRV11 UltimosMovimientos: teléfono con espacios");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "El teléfono no puede tener espacios al inicio o al final.", datos = null });
            }

            if (!TelefonoCR.IsMatch(req.Telefono))
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, $"SRV11 UltimosMovimientos: teléfono inválido ({req.Telefono})");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Teléfono inválido. Debe ser de Costa Rica: 8 dígitos e iniciar con 2,4,5,6,7 u 8.",
                    datos = null
                });
            }

            if (string.IsNullOrWhiteSpace(req.Identificacion))
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV11 UltimosMovimientos: identificación requerida");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "La identificación es requerida.", datos = null });
            }

            if (req.Identificacion != req.Identificacion.Trim())
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV11 UltimosMovimientos: identificación con espacios");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "La identificación no puede tener espacios al inicio o al final.", datos = null });
            }

            // Afiliación en PagosMoviles DB
            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacion is null)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV11: teléfono no afiliado ({req.Telefono})");
                return NotFound(new ApiResponse { codigo = 404, descripcion = "Teléfono no se encuentra afiliado.", datos = null });
            }

            if (!afiliacion.Estado)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV11: teléfono desinscrito ({req.Telefono})");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "El teléfono se encuentra desinscrito.", datos = afiliacion });
            }

            if (!string.Equals(afiliacion.Identificacion, req.Identificacion, StringComparison.OrdinalIgnoreCase))
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV11: identificación no coincide para {req.Telefono}");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "Identificación no coincide con el teléfono afiliado.", datos = null });
            }

            // Token (opcional, por si luego core valida)
            var authHeader = Request.Headers.Authorization.ToString();
            string? token = null;
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = authHeader["Bearer ".Length..].Trim();

            // Llamada a Core SRV16
            CoreSrvClient.ApiResponse<List<CoreSrvClient.MovimientoCoreDto>>? coreResp;

            try
            {
                coreResp = await _core.UltimosMovimientosAsync(
                    afiliacion.Identificacion,
                    afiliacion.NumeroCuenta,
                    token
                );
            }
            catch (HttpRequestException ex)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV11: error conectando a Core. {ex.Message}");
                return StatusCode(502, new ApiResponse
                {
                    codigo = 502,
                    descripcion = $"No fue posible conectar con Core Bancario: {ex.Message}",
                    datos = null
                });
            }

            if (coreResp is null)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, "SRV11: Core devolvió null");
                return StatusCode(502, new ApiResponse { codigo = 502, descripcion = "No fue posible conectar con Core Bancario.", datos = null });
            }

            if (!coreResp.Success || coreResp.Data is null)
            {
                var msg = string.IsNullOrWhiteSpace(coreResp.Message)
                    ? "No fue posible consultar movimientos en Core Bancario."
                    : coreResp.Message;

                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV11: Core FAIL. Msg={msg}");
                return NotFound(new ApiResponse { codigo = 404, descripcion = msg, datos = null });
            }

            // Map a tu formato de salida (mismo que ya vienes usando)
            var movimientos = coreResp.Data.Select(x => new MovimientoDto
            {
                TipoMovimiento = x.TipoMovimiento,
                Monto = x.Monto,
                Fecha = x.Fecha
            }).ToList();

            await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV11 OK Tel={req.Telefono} Cuenta={afiliacion.NumeroCuenta}");

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = new { afiliacion, movimientos }
            });
        }
        catch (Exception ex)
        {
            await SafeBitacora(usuarioBitacora, $"SRV11 ERROR: {ex.Message}");
            return StatusCode(500, new ApiResponse { codigo = 500, descripcion = "Error interno del servidor", datos = null });
        }
    }

    
    // SRV10 - Desinscripción de pagos móviles
    // POST /accounts/unsubscribe
    // Body: { telefono, identificacion, numeroCuenta }  (como el service original)
    

    [AllowAnonymous] //quitar cuando este implementado el loginnnnnnn
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Desinscribir([FromBody] CancelarSuscripcionRequest req)
    {
        var usuarioBitacora = req?.Identificacion ?? "SYSTEM";

        try
        {
            if (req is null)
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV10 Desinscribir: body nulo");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "Debe enviar el body del request.", datos = null });
            }

            // Validaciones mínimas (las del service original)
            if (string.IsNullOrWhiteSpace(req.Telefono) ||
                string.IsNullOrWhiteSpace(req.Identificacion) ||
                string.IsNullOrWhiteSpace(req.NumeroCuenta))
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, "SRV10 Desinscribir: datos incompletos");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "Debe enviar telefono, identificacion y numeroCuenta.", datos = null });
            }

            if (req.Telefono != req.Telefono.Trim() || req.Identificacion != req.Identificacion.Trim() || req.NumeroCuenta != req.NumeroCuenta.Trim())
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, "SRV10 Desinscribir: espacios al inicio/fin");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "No se permiten espacios al inicio o al final.", datos = null });
            }

            if (!TelefonoCR.IsMatch(req.Telefono))
            {
                await _repo.RegistrarBitacoraAsync(usuarioBitacora, $"SRV10 Desinscribir: teléfono inválido ({req.Telefono})");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Teléfono inválido. Debe ser de Costa Rica: 8 dígitos e iniciar con 2,4,5,6,7 u 8.",
                    datos = null
                });
            }

            // Debe existir afiliación
            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);
            if (afiliacion is null)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV10 Desinscribir: teléfono no afiliado ({req.Telefono})");
                return NotFound(new ApiResponse { codigo = 404, descripcion = "Teléfono no se encuentra afiliado.", datos = null });
            }

            // Debe coincidir identificación y cuenta
            if (!string.Equals(afiliacion.Identificacion, req.Identificacion, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(afiliacion.NumeroCuenta, req.NumeroCuenta, StringComparison.OrdinalIgnoreCase))
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV10 Desinscribir: datos no coinciden. Tel={req.Telefono}");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "Datos incorrectos.", datos = null });
            }

            // Si ya está desinscrito, responde OK idempotente
            if (!afiliacion.Estado)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV10 Desinscribir: ya desinscrito Tel={req.Telefono}");
                return Ok(new ApiResponse
                {
                    codigo = 200,
                    descripcion = "El teléfono ya se encontraba desinscrito.",
                    datos = new { telefono = req.Telefono, estado = false }
                });
            }

            // EJECUTA SRV10 REAL (update Estado=0 con validaciones)
            var rows = await _repo.CancelarSuscripcionAsync(req.Telefono, req.Identificacion, req.NumeroCuenta);

            if (rows == 0)
            {
                await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV10 Desinscribir: update no afectó filas Tel={req.Telefono}");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "Datos incorrectos.", datos = null });
            }

            await _repo.RegistrarBitacoraAsync(req.Identificacion, $"SRV10 OK: Desinscripción Tel={req.Telefono}");

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Desinscripción realizada",
                datos = new { telefono = req.Telefono, estado = false }
            });
        }
        catch (Exception ex)
        {
            await SafeBitacora(usuarioBitacora, $"SRV10 ERROR: {ex.Message}");
            return StatusCode(500, new ApiResponse { codigo = 500, descripcion = "Error interno del servidor", datos = null });
        }
    }

    private async Task SafeBitacora(string usuario, string mensaje)
    {
        try { await _repo.RegistrarBitacoraAsync(usuario, mensaje); }
        catch { /* no reventar por bitácora */ }
    }
}