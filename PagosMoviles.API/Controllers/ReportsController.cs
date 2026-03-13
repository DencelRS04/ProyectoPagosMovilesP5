using Microsoft.AspNetCore.Mvc;
using PagosMoviles.API.Data;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Helpers;

namespace PagosMoviles.API.Controllers;

[ApiController]
[Route("reports")]
[Tags("Reportes")]
public class ReportsController : ControllerBase
{
    private readonly PagosMovilesRepository _repo;

    public ReportsController(PagosMovilesRepository repo)
    {
        _repo = repo;
    }

    // SRV17
    // GET /reports/daily-transactions?date=2026-02-28
    [HttpGet("daily-transactions")]
    public async Task<IActionResult> DailyTransactions([FromQuery] DateTime? date)
    {
        var d = (date ?? DateTime.Today).Date;

        try
        {
            var trans = await _repo.GetTransaccionesDiariasAsync(d);
            var totals = await _repo.GetTotalesTransaccionesDiariasAsync(d);

            var reporte = new DailyReportDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                Cantidad = totals.Cantidad,
                Total = totals.Total,
                Transacciones = trans
            };

            await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV17 Reporte diario {d:yyyy-MM-dd}");

            return Ok(new ApiResponse
            {
                codigo = 200,
                descripcion = "Reporte generado",
                datos = reporte
            });
        }
        catch (InvalidOperationException ex)
        {
            // Ej: connection strings faltantes
            await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV17 ERROR CONFIG: {ex.Message}");

            return StatusCode(500, new ApiResponse
            {
                codigo = 500,
                descripcion = ex.Message,
                datos = null
            });
        }
        catch (Exception ex)
        {
            await _repo.RegistrarBitacoraAsync("SYSTEM", $"SRV17 ERROR: {ex.Message}");

            return StatusCode(500, new ApiResponse
            {
                codigo = 500,
                descripcion = "Error interno del servidor",
                datos = null
            });
        }
    }
}