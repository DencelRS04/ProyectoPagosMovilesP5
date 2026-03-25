using Microsoft.AspNetCore.Mvc;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Services;

namespace PagosMoviles.CoreBancarioService.Controllers
{
    [ApiController]
    [Route("core/client")]
    public class CoreClienteController : ControllerBase
    {
        private readonly CoreClienteService _service;

        public CoreClienteController(CoreClienteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var clientes = await _service.ListarAsync();
            return Ok(new
            {
                descripcion = "Clientes obtenidos correctamente",
                datos = clientes
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _service.ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                return NotFound(new
                {
                    descripcion = "Cliente no encontrado",
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                descripcion = "Cliente obtenido correctamente",
                datos = cliente
            });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ClienteDto dto)
        {
            var resultado = await _service.CrearAsync(dto);

            if (!resultado.ok)
            {
                return BadRequest(new
                {
                    descripcion = resultado.mensaje,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                descripcion = resultado.mensaje,
                datos = resultado.cliente
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ClienteDto dto)
        {
            var resultado = await _service.ActualizarAsync(id, dto);

            if (!resultado.ok)
            {
                return BadRequest(new
                {
                    descripcion = resultado.mensaje,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                descripcion = resultado.mensaje,
                datos = (object?)null
            });
        }

        [HttpGet("exists")]
        public async Task<IActionResult> ExisteCliente([FromQuery] string identificacion)
        {
            var existe = await _service.ExistePorIdentificacionAsync(identificacion);

            return Ok(new
            {
                descripcion = "Consulta realizada correctamente",
                datos = new
                {
                    existe
                }
            });
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _service.EliminarAsync(id);

            if (!resultado.ok)
            {
                return BadRequest(new
                {
                    descripcion = resultado.mensaje,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                descripcion = resultado.mensaje,
                datos = (object?)null
            });
        }
    }
}