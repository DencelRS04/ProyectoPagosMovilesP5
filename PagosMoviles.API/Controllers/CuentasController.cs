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

    private static readonly Regex TelefonoCR =
        new(@"^(?:2|4|5|6|7|8)\d{7}$", RegexOptions.Compiled);

    public CuentasController(PagosMovilesRepository repo, CoreSrvClient core)
    {
        _repo = repo;
        _core = core;
    }

    // =========================================================
    // SRV13 - Consultar saldo
    // POST /accounts/balance
    // =========================================================
    [HttpPost("balance")]
    public async Task<IActionResult> ConsultarSaldo([FromBody] ConsultarSaldoRequest req)
    {
        var usuarioBitacora = req?.Identificacion ?? "SYSTEM";

        try
        {
            if (req is null)
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Debe enviar el body del request."
                });

            if (string.IsNullOrWhiteSpace(req.Telefono))
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El teléfono es requerido."
                });

            if (!TelefonoCR.IsMatch(req.Telefono))
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Teléfono inválido."
                });

            if (string.IsNullOrWhiteSpace(req.Identificacion))
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "La identificación es requerida."
                });

            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacion is null)
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Teléfono no afiliado."
                });

            if (!string.Equals(afiliacion.Identificacion, req.Identificacion))
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Identificación no coincide."
                });

            if (!afiliacion.Estado)
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El teléfono se encuentra desinscrito."
                });

            var saldo = await _repo.GetSaldoAsync(
                afiliacion.Identificacion,
                afiliacion.NumeroCuenta
            );

            if (saldo is null)
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Saldo no encontrado."
                });

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
            await SafeBitacora(usuarioBitacora, ex.Message);

            return StatusCode(500, new ApiResponse
            {
                codigo = 500,
                descripcion = "Error interno del servidor"
            });
        }
    }

    // =========================================================
    // SRV11 - Últimos movimientos
    // POST /accounts/transactions
    // =========================================================
    [HttpPost("transactions")]
    public async Task<IActionResult> UltimosMovimientos([FromBody] ConsultarMovimientosRequest req)
    {
        var usuarioBitacora = req?.Identificacion ?? "SYSTEM";

        try
        {
            if (req is null)
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Debe enviar el body del request."
                });

            if (!TelefonoCR.IsMatch(req.Telefono))
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Teléfono inválido."
                });

            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacion is null)
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Teléfono no afiliado."
                });

            var coreResp = await _core.UltimosMovimientosAsync(
                afiliacion.Identificacion,
                afiliacion.NumeroCuenta,
                null
            );

            if (!coreResp.Success || coreResp.Data is null)
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "No fue posible consultar movimientos."
                });

            var movimientos = coreResp.Data.Select(x => new MovimientoDto
            {
                TipoMovimiento = x.TipoMovimiento,
                Monto = x.Monto,
                Fecha = x.Fecha
            });

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = movimientos
            });
        }
        catch (Exception ex)
        {
            await SafeBitacora(usuarioBitacora, ex.Message);

            return StatusCode(500, new ApiResponse
            {
                codigo = 500,
                descripcion = "Error interno del servidor"
            });
        }
    }
    // =========================================================
    // SRV11 - Últimos 5 movimientos por teléfono
    // GET /accounts/transactions/{telefono}
    // =========================================================
    [HttpGet("transactions/{telefono}")]
    public async Task<IActionResult> UltimosMovimientosPorTelefono(string telefono)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV11 GET: teléfono requerido");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "El teléfono es requerido.", datos = null });
            }

            if (telefono != telefono.Trim())
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", "SRV11 GET: teléfono con espacios");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "El teléfono no puede tener espacios.", datos = null });
            }

            if (!TelefonoCR.IsMatch(telefono))
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV11 GET: teléfono inválido ({telefono})");
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Teléfono inválido. Debe ser de Costa Rica: 8 dígitos e iniciar con 2,4,5,6,7 u 8.",
                    datos = null
                });
            }

            // Buscar afiliación por teléfono
            var afiliacion = await _repo.GetPagoMovilMiniByTelefonoAsync(telefono);

            if (afiliacion is null)
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV11 GET: teléfono no afiliado ({telefono})");
                return NotFound(new ApiResponse { codigo = 404, descripcion = "Teléfono no se encuentra afiliado.", datos = null });
            }

            if (!afiliacion.Estado)
            {
                await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV11 GET: teléfono desinscrito ({telefono})");
                return BadRequest(new ApiResponse { codigo = 400, descripcion = "El teléfono se encuentra desinscrito.", datos = null });
            }

            // Token opcional
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
                await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV11 GET: error conectando a Core. {ex.Message}");
                return StatusCode(502, new ApiResponse
                {
                    codigo = 502,
                    descripcion = $"No fue posible conectar con Core Bancario: {ex.Message}",
                    datos = null
                });
            }

            if (coreResp is null || !coreResp.Success || coreResp.Data is null)
            {
                var msg = coreResp?.Message ?? "No fue posible consultar movimientos en Core Bancario.";
                await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV11 GET: Core FAIL. Msg={msg}");
                return NotFound(new ApiResponse { codigo = 404, descripcion = msg, datos = null });
            }

            var movimientos = coreResp.Data.Select(x => new MovimientoDto
            {
                TipoMovimiento = x.TipoMovimiento,
                Monto = x.Monto,
                Fecha = x.Fecha
            }).ToList();

            await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV11 GET OK Tel={telefono} Cuenta={afiliacion.NumeroCuenta}");

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = new { afiliacion, movimientos }
            });
        }
        catch (Exception ex)
        {
            await SafeBitacora("SYSTEM", $"SRV11 GET ERROR: {ex.Message}");
            return StatusCode(500, new ApiResponse { codigo = 500, descripcion = "Error interno del servidor", datos = null });
        }
    }

    // =========================================================
    // SRV10 - Desinscripción
    // POST /accounts/unsubscribe
    // =========================================================
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Desinscribir([FromBody] CancelarSuscripcionRequest req)
    {
        Console.WriteLine("==== SRV10 Desinscribir iniciado ====");

        var usuarioBitacora = req?.Identificacion ?? "SYSTEM";

        try
        {
            if (req is null)
            {
                Console.WriteLine("Request viene null");

                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Debe enviar el body del request."
                });
            }

            Console.WriteLine($"Telefono recibido: {req.Telefono}");
            Console.WriteLine($"Identificacion recibida: {req.Identificacion}");
            Console.WriteLine($"Cuenta recibida: {req.NumeroCuenta}");

            var afiliacionAntes = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacionAntes is null)
            {
                Console.WriteLine("No existe afiliación para ese teléfono");

                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Teléfono no afiliado."
                });
            }

            Console.WriteLine($"Estado antes del update: {afiliacionAntes.Estado}");

            var rows = await _repo.CancelarSuscripcionAsync(
                req.Telefono,
                req.Identificacion,
                req.NumeroCuenta
            );

            Console.WriteLine($"Filas afectadas: {rows}");

            if (rows == 0)
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Datos incorrectos."
                });
            }

            var afiliacionDespues = await _repo.GetPagoMovilMiniByTelefonoAsync(req.Telefono);

            if (afiliacionDespues is null)
            {
                return StatusCode(500, new ApiResponse
                {
                    codigo = 500,
                    descripcion = "No fue posible verificar el estado de la desinscripción."
                });
            }

            Console.WriteLine($"Estado después del update: {afiliacionDespues.Estado}");

            if (afiliacionDespues.Estado)
            {
                return StatusCode(500, new ApiResponse
                {
                    codigo = 500,
                    descripcion = "La operación respondió correctamente, pero el estado no cambió en base de datos."
                });
            }

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Desinscripción realizada",
                datos = new
                {
                    telefono = req.Telefono,
                    estado = false
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR EN SRV10");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            await SafeBitacora(usuarioBitacora, ex.Message);

            return StatusCode(500, new ApiResponse
            {
                codigo = 500,
                descripcion = "Error interno del servidor"
            });
        }
    }

    private async Task SafeBitacora(string usuario, string mensaje)
    {
        try
        {
            await _repo.RegistrarBitacoraAsync(usuario, mensaje);
        }
        catch { }
    }
}