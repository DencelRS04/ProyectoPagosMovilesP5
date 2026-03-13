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
[Route("entidad")]
public class EntidadController : ControllerBase
{
    private readonly PagosMovilesDbContext _context;
    private readonly BitacoraService _bitacora;

    public EntidadController(PagosMovilesDbContext context, BitacoraService bitacora)
    {
        _context = context;
        _bitacora = bitacora;
    }

    private bool NombreValido(string nombre) => Regex.IsMatch(nombre, @"^[a-zA-Z\s]+$");
    private bool CodigoValido(string codigo) => Regex.IsMatch(codigo, @"^[a-zA-Z0-9]+$");

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] EntidadBancariaDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.CodigoEntidad) || string.IsNullOrWhiteSpace(dto.NombreInstitucion))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Todos los campos son obligatorios", datos = null });

        if (!CodigoValido(dto.CodigoEntidad))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "El código solo permite letras y números", datos = null });

        if (!NombreValido(dto.NombreInstitucion))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "El nombre solo puede contener letras y espacios", datos = null });

        if (_context.EntidadesBancarias.Any(e => e.CodigoEntidad == dto.CodigoEntidad))
            return Conflict(new ApiResponse { codigo = 409, descripcion = "Ya existe una entidad con ese código", datos = null });

        var entidad = new EntidadBancaria
        {
            CodigoEntidad = dto.CodigoEntidad,
            NombreInstitucion = dto.NombreInstitucion
        };

        _context.EntidadesBancarias.Add(entidad);
        await _context.SaveChangesAsync();

        await _bitacora.Registrar("admin", "Creó entidad", null, entidad);

        return StatusCode(201, new ApiResponse
        {
            codigo = 201,
            descripcion = "Entidad creada correctamente",
            datos = entidad
        });
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await Task.FromResult(_context.EntidadesBancarias.ToList());

        await _bitacora.Registrar("admin", "Consultó entidades");

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = lista
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var entidad = _context.EntidadesBancarias.Find(id);

        if (entidad == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Entidad no encontrada", datos = null });

        await _bitacora.Registrar("admin", $"Consultó entidad ID {id}");

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Consulta exitosa",
            datos = entidad
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] EntidadBancariaDto dto)
    {
        var entidad = _context.EntidadesBancarias.Find(id);

        if (entidad == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Entidad no encontrada", datos = null });

        if (dto == null || string.IsNullOrWhiteSpace(dto.CodigoEntidad) || string.IsNullOrWhiteSpace(dto.NombreInstitucion))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Todos los campos son obligatorios", datos = null });

        if (!CodigoValido(dto.CodigoEntidad))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Código inválido", datos = null });

        if (!NombreValido(dto.NombreInstitucion))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Nombre inválido", datos = null });

        var anterior = new { entidad.EntidadId, entidad.CodigoEntidad, entidad.NombreInstitucion };

        entidad.CodigoEntidad = dto.CodigoEntidad;
        entidad.NombreInstitucion = dto.NombreInstitucion;

        await _context.SaveChangesAsync();
        await _bitacora.Registrar("admin", $"Modificó entidad ID {id}", anterior, entidad);

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Entidad modificada correctamente",
            datos = entidad
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var entidad = await _context.EntidadesBancarias
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EntidadId == id);

        if (entidad == null)
        {
            return NotFound(new ApiResponse
            {
                codigo = 404,
                descripcion = "Entidad no encontrada",
                datos = null
            });
        }

        // 🔹 Registrar en bitácora antes de eliminar
        await _bitacora.Registrar(
            usuario: "admin", // aquí deberías sacar el usuario del token
            accion: "Eliminó entidad bancaria",
            anterior: entidad,
            actual: null
        );

        _context.EntidadesBancarias.Remove(entidad);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Entidad eliminada correctamente",
            datos = entidad 
        });
    }
}
