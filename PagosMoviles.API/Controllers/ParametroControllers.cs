using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Data;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Helpers;
using PagosMoviles.API.Models;
using PagosMoviles.API.Services;
using System.Text.RegularExpressions;

namespace PagosMoviles.API.Controllers;

[ApiController]
[Route("parametro")]
[AllowAnonymous] // TODO: Remover cuando Login esté implementado
public class ParametroController : ControllerBase
{
    private readonly PagosMovilesDbContext _context;
    private readonly BitacoraService _bitacora;

    public ParametroController(PagosMovilesDbContext context, BitacoraService bitacora)
    {
        _context = context;
        _bitacora = bitacora;
    }

    [HttpGet]
    public IActionResult ObtenerTodos()
    {
        var lista = _context.Parametros.ToList();
        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = lista
        });
    }

    [HttpGet("{id}")]
    public IActionResult Obtener(string id)
    {
        var parametro = _context.Parametros.Find(id);
        if (parametro == null)
        {
            return NotFound(new ApiResponse
            {
                codigo = 404,
                descripcion = "Parámetro no encontrado",
                datos = null
            });
        }

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = parametro
        });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ParametroDTO dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ParametroId) || string.IsNullOrWhiteSpace(dto.Valor))
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "Todos los campos son obligatorios",
                datos = null
            });
        }

        if (!Regex.IsMatch(dto.ParametroId, @"^[A-Z]{1,10}$"))
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "El identificador debe ser mayúsculas (1 a 10 letras)",
                datos = null
            });
        }

        if (dto.Valor.Length > 500)
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "El valor excede 500 caracteres",
                datos = null
            });
        }

        var existe = _context.Parametros.Any(p => p.ParametroId == dto.ParametroId);
        if (existe)
        {
            return Conflict(new ApiResponse
            {
                codigo = 409,
               descripcion = "Ya existe un parámetro con ese identificador",
                datos = null
            });
        }

        var parametro = new Parametro { ParametroId = dto.ParametroId, Valor = dto.Valor };
        _context.Parametros.Add(parametro);
        await _context.SaveChangesAsync();

        return StatusCode(201, new ApiResponse
        {
            codigo = 201,
            descripcion = "Parámetro creado correctamente",
            datos = parametro
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(string id, [FromBody] ParametroDTO dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Valor))
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "El valor es obligatorio",
                datos = null
            });
        }

        var parametro = _context.Parametros.Find(id);
        if (parametro == null)
        {
            return NotFound(new ApiResponse
            {
                codigo = 404,
                descripcion = "Parámetro no encontrado",
                datos = null
            });
        }

        if (dto.Valor.Length > 500)
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = "El valor excede 500 caracteres",
                datos = null
            });
        }

        parametro.Valor = dto.Valor;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Parámetro modificado correctamente",
            datos = parametro
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        var parametro = await _context.Parametros
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ParametroId == id);

        if (parametro == null)
        {
            return NotFound(new ApiResponse
            {
                codigo = 404,
                descripcion = "Parámetro no encontrado",
                datos = null
            });
        }

        // 🔹 Registrar en bitácora antes de eliminar
        await _bitacora.Registrar(
            usuario: "admin", 
            accion: "Eliminó parámetro",
            anterior: parametro,
            actual: null
        );

        // 🔹 Eliminar
        _context.Parametros.Remove(new Parametro { ParametroId = id });
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Parámetro eliminado correctamente",
            datos = parametro   
        });
    }
}
