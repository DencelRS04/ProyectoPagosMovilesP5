using Microsoft.AspNetCore.Mvc;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Services;

namespace PagosMoviles.API.Controllers;

[ApiController]
[Route("transactions")]  // ← antes era [Route("api/[controller]")]
[Tags("Transacciones")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionLogic _logic;

    public TransactionsController(ITransactionLogic logic)
    {
        _logic = logic;
    }

    private IActionResult ValidationError()
    {
        // Devuelve mensajes específicos por campo
        var errores = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value!.Errors.Select(e => e.ErrorMessage).ToList()
            );

        return BadRequest(new ApiResponse
        {
            codigo = 400,
            descripcion = "Validación fallida",
            datos = errores
        });
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessTransaction([FromBody] TransactionRequestDto transaction)
    {
        if (!ModelState.IsValid) return ValidationError();

        var response = await _logic.ProcessTransaction(transaction);

        return StatusCode(response.StatusCode, new ApiResponse
        {
            codigo = response.StatusCode,
            descripcion = response.Message,
            datos = response.ResponseObject
        });
    }
    [HttpGet("por-fecha")]
    public async Task<IActionResult> ObtenerPorFecha([FromQuery] string fecha)
    {
        if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out DateTime fechaParsed))
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "Formato de fecha inválido. Use yyyy-MM-dd"
            });
        }

        var response = await _logic.ObtenerPorFecha(fechaParsed);
        return StatusCode(response.StatusCode, new ApiResponse
        {
            codigo = response.StatusCode,
            descripcion = response.Message,
            datos = response.ResponseObject
        });
    }

    [HttpPost("route")]
    public async Task<IActionResult> RouteTransaction([FromBody] TransactionRouteDto transaction)
    {
        if (!ModelState.IsValid) return ValidationError();

        var response = await _logic.RouteTransaction(transaction);

        return StatusCode(response.StatusCode, new ApiResponse
        {
            codigo = response.StatusCode,
            descripcion = response.Message,
            datos = response.ResponseObject
        });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendTransaction([FromBody] TransactionSendDto transaction)
    {
        if (!ModelState.IsValid) return ValidationError();

        var response = await _logic.SendTransaction(transaction);

        // Si tu lógica devuelve 201, lo respeta
        return StatusCode(response.StatusCode, new ApiResponse
        {
            codigo = response.StatusCode,
            descripcion = response.Message,
            datos = response.ResponseObject
        });
    }
}

// ✅ Usa tu ApiResponse ya existente
public class ApiResponse
{
    public int codigo { get; set; }
    public string descripcion { get; set; } = "";
    public object? datos { get; set; }
}