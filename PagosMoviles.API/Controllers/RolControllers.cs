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
[Route("rol")]
public class RolController : ControllerBase
{
    private readonly PagosMovilesDbContext _context;
    private readonly BitacoraService _bitacora;

    public RolController(PagosMovilesDbContext context, BitacoraService bitacora)
    {
        _context = context;
        _bitacora = bitacora;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] RolDTO dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Nombre) || dto.Pantallas == null || !dto.Pantallas.Any())
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Nombre y pantallas son obligatorios", datos = null });

        if (!Regex.IsMatch(dto.Nombre, @"^[a-zA-Z0-9 ]+$"))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Nombre inválido (solo letras, números y espacios)", datos = null });

        // ✅ Validar pantallas existen
        var pantallasExistentes = await _context.Pantallas
            .Where(p => dto.Pantallas.Contains(p.PantallaId))
            .Select(p => p.PantallaId)
            .ToListAsync();

        var faltantes = dto.Pantallas.Except(pantallasExistentes).ToList();
        if (faltantes.Any())
            return BadRequest(new ApiResponse { codigo = 400, descripcion = $"Pantallas inválidas: {string.Join(",", faltantes)}", datos = null });

        var rol = new Rol { Nombre = dto.Nombre };
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();

        foreach (var p in dto.Pantallas.Distinct())
            _context.RolPantallas.Add(new RolPantalla { RolId = rol.RolId, PantallaId = p });

        await _context.SaveChangesAsync();

        await _bitacora.Registrar("admin", "Creó rol", null, new { rol, pantallas = dto.Pantallas });

        return StatusCode(201, new ApiResponse { codigo = 201, descripcion = "Rol creado", datos = rol });
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var roles = await _context.Roles.ToListAsync();
        return Ok(new ApiResponse { codigo = 200, descripcion = "OK", datos = roles });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var rol = await _context.Roles.FindAsync(id);
        if (rol == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Rol no encontrado", datos = null });

        return Ok(new ApiResponse { codigo = 200, descripcion = "OK", datos = rol });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] RolDTO dto)
    {
        var rol = await _context.Roles.FindAsync(id);
        if (rol == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Rol no encontrado", datos = null });

        if (dto == null || string.IsNullOrWhiteSpace(dto.Nombre) || dto.Pantallas == null || !dto.Pantallas.Any())
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Nombre y pantallas son obligatorios", datos = null });

        if (!Regex.IsMatch(dto.Nombre, @"^[a-zA-Z0-9 ]+$"))
            return BadRequest(new ApiResponse { codigo = 400, descripcion = "Nombre inválido (solo letras, números y espacios)", datos = null });

        var pantallasExistentes = await _context.Pantallas
            .Where(p => dto.Pantallas.Contains(p.PantallaId))
            .Select(p => p.PantallaId)
            .ToListAsync();

        var faltantes = dto.Pantallas.Except(pantallasExistentes).ToList();
        if (faltantes.Any())
        {
            return BadRequest(new ApiResponse
            {
                codigo = 400,
                descripcion = $"No existe(n) PantallaId: {string.Join(",", faltantes)}",
                datos = null
            });
        }

        var anterior = new
        {
            rol.RolId,
            rol.Nombre,
            pantallas = await _context.RolPantallas
                .Where(x => x.RolId == id)
                .Select(x => x.PantallaId)
                .ToListAsync()
        };

        rol.Nombre = dto.Nombre;

        var relaciones = _context.RolPantallas.Where(r => r.RolId == id);
        _context.RolPantallas.RemoveRange(relaciones);

        foreach (var p in dto.Pantallas.Distinct())
            _context.RolPantallas.Add(new RolPantalla { RolId = id, PantallaId = p });

        await _context.SaveChangesAsync();

        await _bitacora.Registrar("admin", "Modificó rol", anterior, new { rol, pantallas = dto.Pantallas });

        return Ok(new ApiResponse { codigo = 200, descripcion = "Rol modificado", datos = rol });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        // Traer el rol (sin tracking para devolverlo limpio)
        var rol = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RolId == id);

        if (rol == null)
            return NotFound(new ApiResponse { codigo = 404, descripcion = "Rol no encontrado", datos = null });

        // (Opcional) Traer pantallas asociadas para devolverlas también
        var pantallas = await _context.RolPantallas
            .AsNoTracking()
            .Where(rp => rp.RolId == id)
            .Select(rp => rp.PantallaId)
            .ToListAsync();

        // Bitácora ANTES de borrar (más seguro)
        await _bitacora.Registrar(
            usuario: "admin",
            accion: "Eliminó rol",
            anterior: new { rol, pantallas },
            actual: null
        );

        // 1) Borrar relaciones primero
        var relaciones = await _context.RolPantallas
            .Where(rp => rp.RolId == id)
            .ToListAsync();

        if (relaciones.Any())
            _context.RolPantallas.RemoveRange(relaciones);

        // 2) Borrar rol
        _context.Roles.Remove(new Rol { RolId = id });

        // 3) Guardar UNA sola vez
        await _context.SaveChangesAsync();

        //  Respuesta con lo eliminado
        return Ok(new ApiResponse
        {
            codigo = 200,
            descripcion = "Rol eliminado correctamente",
            datos = new
            {
                rol,
                pantallas
            }
        });
    }

}
