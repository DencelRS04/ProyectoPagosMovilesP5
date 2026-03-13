using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Data;
using PagosMoviles.API.Helpers;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Services;

namespace PagosMoviles.API.Controllers;

[ApiController]
[Route("bitacora")]
public class BitacoraController : ControllerBase
{
    private readonly PagosMovilesDbContext _context;
    private readonly BitacoraService _bitacora;

    public BitacoraController(PagosMovilesDbContext context, BitacoraService bitacora)
    {
        _context = context;
        _bitacora = bitacora;
    }

    // GET /bitacora
    [HttpGet]
    public IActionResult ListarTodo()
    {
        var bitacoras = _context.Bitacoras
            .OrderByDescending(b => b.Fecha)
            .ToList();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = bitacoras
        });
    }

    // GET /bitacora/fecha?inicio=2026-02-19&fin=2026-02-19
    [HttpGet("fecha")]
    public IActionResult FiltrarPorFecha([FromQuery] DateTime inicio, [FromQuery] DateTime fin)
    {
        // Validación de rango
        if (inicio.Date > fin.Date)
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "Rango de fechas inválido: inicio no puede ser mayor que fin",
                datos = null
            });
        }

        // Incluye TODO el día fin (hasta 23:59:59.9999999)
        var desde = inicio.Date;
        var hasta = fin.Date.AddDays(1).AddTicks(-1);

        var resultado = _context.Bitacoras
            .Where(b => b.Fecha >= desde && b.Fecha <= hasta)
            .OrderByDescending(b => b.Fecha)
            .ToList();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = resultado
        });
    }

    // GET /bitacora/buscar?texto=creó
    [HttpGet("buscar")]
    public IActionResult BuscarPorTexto([FromQuery] string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "Debe ingresar un texto.",
                datos = null
            });
        }

        // Ignora tildes y mayúsculas (CI_AI)
        var resultado = _context.Bitacoras
            .Where(b =>
                EF.Functions.Like(
                    EF.Functions.Collate(b.Descripcion, "Latin1_General_CI_AI"),
                    $"%{texto}%"
                )
            )
            .OrderByDescending(b => b.Fecha)
            .ToList();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = resultado
        });
    }

    // GET /bitacora/usuario?usuario=admin
    [HttpGet("usuario")]
    public IActionResult FiltrarPorUsuario([FromQuery] string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario))
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "Debe ingresar un usuario.",
                datos = null
            });
        }

        var resultado = _context.Bitacoras
            .Where(b => b.Usuario == usuario)
            .OrderByDescending(b => b.Fecha)
            .ToList();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = resultado
        });
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] BitacoraDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Usuario) || string.IsNullOrWhiteSpace(dto.Accion))
            return BadRequest();

        await _bitacora.Registrar(dto.Usuario, dto.Accion, dto.Anterior, dto.Actual);
        return Ok();
    }
}
