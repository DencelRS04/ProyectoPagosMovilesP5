using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Data;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Helpers;
using PagosMoviles.API.Models;
using PagosMoviles.API.Services;
using System.Text.RegularExpressions;

namespace PagosMoviles.API.Controllers
{
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

        private bool NombreValido(string nombre) =>
            Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s\.\-&]+$");

        private bool CodigoValido(string codigo) =>
            Regex.IsMatch(codigo, @"^[a-zA-Z0-9]+$");

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] EntidadBancariaDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.CodigoEntidad) ||
                string.IsNullOrWhiteSpace(dto.NombreInstitucion))
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Todos los campos son obligatorios",
                    datos = null
                });
            }

            dto.CodigoEntidad = dto.CodigoEntidad.Trim();
            dto.NombreInstitucion = dto.NombreInstitucion.Trim();

            if (!CodigoValido(dto.CodigoEntidad))
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El código solo permite letras y números",
                    datos = null
                });
            }

            if (!NombreValido(dto.NombreInstitucion))
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "El nombre contiene caracteres no permitidos",
                    datos = null
                });
            }

            var existeCodigo = await _context.EntidadesBancarias
                .AnyAsync(e => e.CodigoEntidad == dto.CodigoEntidad);

            if (existeCodigo)
            {
                return Conflict(new ApiResponse
                {
                    codigo = 409,
                    descripcion = "Ya existe una entidad con ese código",
                    datos = null
                });
            }

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
            var lista = await _context.EntidadesBancarias
                .OrderBy(e => e.EntidadId)
                .ToListAsync();

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
            var entidad = await _context.EntidadesBancarias.FindAsync(id);

            if (entidad == null)
            {
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Entidad no encontrada",
                    datos = null
                });
            }

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
            var entidad = await _context.EntidadesBancarias.FindAsync(id);

            if (entidad == null)
            {
                return NotFound(new ApiResponse
                {
                    codigo = 404,
                    descripcion = "Entidad no encontrada",
                    datos = null
                });
            }

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.CodigoEntidad) ||
                string.IsNullOrWhiteSpace(dto.NombreInstitucion))
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Todos los campos son obligatorios",
                    datos = null
                });
            }

            dto.CodigoEntidad = dto.CodigoEntidad.Trim();
            dto.NombreInstitucion = dto.NombreInstitucion.Trim();

            if (!CodigoValido(dto.CodigoEntidad))
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Código inválido",
                    datos = null
                });
            }

            if (!NombreValido(dto.NombreInstitucion))
            {
                return BadRequest(new ApiResponse
                {
                    codigo = 400,
                    descripcion = "Nombre inválido",
                    datos = null
                });
            }

            var codigoDuplicado = await _context.EntidadesBancarias
                .AnyAsync(e => e.CodigoEntidad == dto.CodigoEntidad && e.EntidadId != id);

            if (codigoDuplicado)
            {
                return Conflict(new ApiResponse
                {
                    codigo = 409,
                    descripcion = "Ya existe otra entidad con ese código",
                    datos = null
                });
            }

            var anterior = new
            {
                entidad.EntidadId,
                entidad.CodigoEntidad,
                entidad.NombreInstitucion
            };

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

            await _bitacora.Registrar("admin", "Eliminó entidad bancaria", entidad, null);

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
}