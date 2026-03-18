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