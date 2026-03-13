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
[Route("pantalla")]
public class PantallaController : ControllerBase
{
    private readonly PagosMovilesDbContext _context;
    private readonly BitacoraService _bitacora;

    public PantallaController(PagosMovilesDbContext context, BitacoraService bitacora)
    {
        _context = context;
        _bitacora = bitacora;
    }

    private bool TextoValido(string texto) => Regex.IsMatch(texto, @"^[a-zA-Z0-9 ]+$");
    private bool IdentificadorValido(string texto) => Regex.IsMatch(texto, @"^[a-zA-Z0-9]+$");

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PantallaDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Identificador) || string.IsNullOrWhiteSpace(dto.Nombre)
            || string.IsNullOrWhiteSpace(dto.Descripcion) || string.IsNullOrWhiteSpace(dto.Ruta))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Todos los campos son obligatorios", datos = null });

        if (!IdentificadorValido(dto.Identificador))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Identificador inválido", datos = null });

        if (!TextoValido(dto.Nombre) || !TextoValido(dto.Descripcion))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Nombre/Descripción inválidos", datos = null });

        if (_context.Pantallas.Any(p => p.Identificador == dto.Identificador))
            return Conflict(new ApiResponse { codigo = 409, descripcion = "Ya existe una pantalla con ese identificador", datos = null });

        var pantalla = new Pantalla
        {
            Identificador = dto.Identificador,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Ruta = dto.Ruta
        };

        _context.Pantallas.Add(pantalla);
        await _context.SaveChangesAsync();

        await _bitacora.Registrar("admin", "Creó pantalla", null, pantalla);

        return StatusCode(201, new ApiResponse { codigo = 201, descripcion = "Pantalla creada correctamente", datos = pantalla });
    }

    [HttpGet]
    public IActionResult Listar()
    {
        var lista = _context.Pantallas.ToList();
        return Ok(new ApiResponse { codigo = 200, descripcion = "Consulta exitosa", datos = lista });
    }

    [HttpGet("{id}")]
    public IActionResult Obtener(int id)
    {
        var pantalla = _context.Pantallas.Find(id);
        if (pantalla == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Pantalla no encontrada", datos = null });

        return Ok(new ApiResponse { codigo = 200, descripcion = "Consulta exitosa", datos = pantalla });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] PantallaDto dto)
    {
        var pantalla = _context.Pantallas.Find(id);
        if (pantalla == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Pantalla no encontrada", datos = null });

        if (dto == null || string.IsNullOrWhiteSpace(dto.Identificador) || string.IsNullOrWhiteSpace(dto.Nombre)
            || string.IsNullOrWhiteSpace(dto.Descripcion) || string.IsNullOrWhiteSpace(dto.Ruta))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Todos los campos son obligatorios", datos = null });

        if (!IdentificadorValido(dto.Identificador) || !TextoValido(dto.Nombre) || !TextoValido(dto.Descripcion))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Datos inválidos", datos = null });

        var anterior = new { pantalla.PantallaId, pantalla.Identificador, pantalla.Nombre, pantalla.Descripcion, pantalla.Ruta };

        pantalla.Identificador = dto.Identificador;
        pantalla.Nombre = dto.Nombre;
        pantalla.Descripcion = dto.Descripcion;
        pantalla.Ruta = dto.Ruta;

        await _context.SaveChangesAsync();
        await _bitacora.Registrar("admin", $"Modificó pantalla ID {id}", anterior, pantalla);

        return Ok(new ApiResponse { codigo = 200, descripcion = "Pantalla modificada correctamente", datos = pantalla });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var pantalla = await _context.Pantallas
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PantallaId == id);

        if (pantalla == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Pantalla no encontrada", datos = null });

        // 1) eliminar relaciones RolPantalla
        var relaciones = await _context.RolPantallas
            .Where(rp => rp.PantallaId == id)
            .ToListAsync();

        if (relaciones.Count > 0)
            _context.RolPantallas.RemoveRange(relaciones);

        // 2) bitácora (antes de borrar)
        await _bitacora.Registrar(
            usuario: "admin", // ideal: sacarlo del token
            accion: "Eliminó pantalla",
            anterior: pantalla,
            actual: null
        );

        // 3) eliminar pantalla
        _context.Pantallas.Remove(new Pantalla { PantallaId = id });
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Pantalla eliminada correctamente",
            datos = pantalla
        });
    }
}
