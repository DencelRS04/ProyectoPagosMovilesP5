using Microsoft.AspNetCore.Mvc;
using PagosMoviles.PagosMovilesService.Auth;
using PagosMoviles.PagosMovilesService.Data;
using PagosMoviles.PagosMovilesService.Dtos;

namespace PagosMoviles.PagosMovilesService.Controllers;

[ApiController]
[Route("reports")]
public class ReportsController : ControllerBase
{
    private readonly PagosMovilesRepository _repo;

    public ReportsController(PagosMovilesRepository repo)
    {
        _repo = repo;
    }

    // SRV17
    [HttpGet("daily-transactions")]
    [SrvAuthorize]
    public async Task<IActionResult> DailyTransactions([FromQuery] DateTime? date)
    {
        var d = (date ?? DateTime.Today).Date;

        var trans = await _repo.GetTransaccionesDiariasAsync(d);
        var totals = await _repo.GetTotalesTransaccionesDiariasAsync(d);

        var reporte = new DailyReportDto
        {
            Date = d.ToString("yyyy-MM-dd"),
            Cantidad = totals.Cantidad,
            Total = totals.Total,
            Transacciones = trans.Select(t => new TransaccionMovilDto
            {
                TransaccionId = t.TransaccionId,
                EntidadOrigen = t.EntidadOrigen,
                EntidadDestino = t.EntidadDestino,
                TelefonoOrigen = t.TelefonoOrigen,
                TelefonoDestino = t.TelefonoDestino,
                Monto = t.Monto,
                Descripcion = t.Descripcion,
                Fecha = t.Fecha
            }).ToList()
        };

        await _repo.RegistrarBitacoraAsync("SYSTEM", $"Reporte diario {d:yyyy-MM-dd}");

        return Ok(new { codigo = 0, reporte });
    }
}