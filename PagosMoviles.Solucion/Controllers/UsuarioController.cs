using Microsoft.AspNetCore.Mvc;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Services;

namespace PagosMoviles.UsuariosService.Controllers
{
    [ApiController]
    [Route("user")]
    [Produces("application/json")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;
        private readonly BitacoraClient _bitacora;

        public UsuarioController(UsuarioService service, BitacoraClient bitacora)
        {
            _service = service;
            _bitacora = bitacora;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var resp = await _service.ObtenerTodosAsync();

            return Ok(new
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = resp.Data
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var resp = await _service.ObtenerPorIdAsync(id);

            if (!resp.Success)
            {
                return NotFound(new
                {
                    codigo = 404,
                    descripcion = resp.Message,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = resp.Data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] UsuarioCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Debe enviar el body del request.",
                    datos = (object?)null
                });
            }

            var resp = await _service.CrearUsuarioAsync(dto);

            if (!resp.Success)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = resp.Message,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = "Usuario creado correctamente.",
                datos = resp.Data
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Debe enviar el body del request.",
                    datos = (object?)null
                });
            }

            var resp = await _service.ActualizarUsuarioAsync(id, dto);

            if (!resp.Success)
            {
                return NotFound(new
                {
                    codigo = 404,
                    descripcion = resp.Message,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = "Usuario actualizado correctamente.",
                datos = resp.Data
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resp = await _service.EliminarUsuarioAsync(id);

            if (!resp.Success)
            {
                return NotFound(new
                {
                    codigo = 404,
                    descripcion = resp.Message,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = "Usuario eliminado correctamente.",
                datos = resp.Data
            });
        }
    }
}