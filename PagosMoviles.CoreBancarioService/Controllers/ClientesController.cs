using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Services;

namespace PagosMoviles.CoreBancarioService.Controllers
{
    [ApiController]
    [Route("core")]
    public class ClientesController : ControllerBase
    {
        private readonly CoreDbContext _db;
        private readonly CoreClienteService _service;
        private readonly CoreGatewayBitacoraClient _bitacora;

        public ClientesController(
            CoreDbContext db,
            CoreClienteService service,
            CoreGatewayBitacoraClient bitacora)
        {
            _db = db;
            _service = service;
            _bitacora = bitacora;
        }

        // SRV19
        // GET /core/client-exists?identificacion=123
        [HttpGet("client-exists")]
        public async Task<IActionResult> ClienteExiste([FromQuery] string identificacion)
        {
            var token = GetBearerToken();

            if (string.IsNullOrWhiteSpace(identificacion))
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SRV19 ClienteExiste: identificación requerida",
                    null,
                    new { identificacion },
                    token
                );

                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Identificación requerida",
                    datos = (object?)null
                });
            }

            var existe = await _db.Clientes.AnyAsync(c => c.Identificacion == identificacion);

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SRV19 ClienteExiste",
                null,
                new { identificacion, existe },
                token
            );

            return Ok(new
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = new { existe }
            });
        }

        // SA10
        // GET /core/client
        [HttpGet("client")]
        public async Task<IActionResult> ListarClientes()
        {
            var token = GetBearerToken();
            var clientes = await _service.ListarAsync();

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SA10 Listar clientes core",
                null,
                new { total = clientes.Count },
                token
            );

            return Ok(new
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = clientes
            });
        }

        // GET /core/client/{id}
        [HttpGet("client/{id:int}")]
        public async Task<IActionResult> ObtenerCliente(int id)
        {
            var token = GetBearerToken();
            var cliente = await _service.ObtenerPorIdAsync(id);

            if (cliente == null)
            {
                await _bitacora.RegistrarAsync(
                    "SYSTEM",
                    "SA10 Obtener cliente no encontrado",
                    null,
                    new { id },
                    token
                );

                return NotFound(new
                {
                    codigo = 404,
                    descripcion = "Cliente no encontrado",
                    datos = (object?)null
                });
            }

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SA10 Obtener cliente por id",
                null,
                new { id },
                token
            );

            return Ok(new
            {
                codigo = 200,
                descripcion = "Consulta exitosa",
                datos = cliente
            });
        }

        // POST /core/client
        [HttpPost("client")]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Datos inválidos",
                    datos = ModelState
                });
            }

            var resultado = await _service.CrearAsync(dto);

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SA10 Crear cliente core",
                null,
                new { dto.Identificacion, resultado.ok, resultado.mensaje },
                token
            );

            if (!resultado.ok)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = resultado.mensaje,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = resultado.mensaje,
                datos = resultado.cliente
            });
        }

        // PUT /core/client/{id}
        [HttpPut("client/{id:int}")]
        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ClienteDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Datos inválidos",
                    datos = ModelState
                });
            }

            var resultado = await _service.ActualizarAsync(id, dto);

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SA10 Actualizar cliente core",
                null,
                new { id, dto.Identificacion, resultado.ok, resultado.mensaje },
                token
            );

            if (!resultado.ok)
            {
                if (resultado.mensaje == "Cliente no encontrado")
                {
                    return NotFound(new
                    {
                        codigo = 404,
                        descripcion = resultado.mensaje,
                        datos = (object?)null
                    });
                }

                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = resultado.mensaje,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = resultado.mensaje,
                datos = (object?)null
            });
        }

        // DELETE /core/client/{id}
        [HttpDelete("client/{id:int}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var token = GetBearerToken();
            var resultado = await _service.EliminarAsync(id);

            await _bitacora.RegistrarAsync(
                "SYSTEM",
                "SA10 Eliminar cliente core",
                null,
                new { id, resultado.ok, resultado.mensaje },
                token
            );

            if (!resultado.ok)
            {
                return NotFound(new
                {
                    codigo = 404,
                    descripcion = resultado.mensaje,
                    datos = (object?)null
                });
            }

            return Ok(new
            {
                codigo = 200,
                descripcion = resultado.mensaje,
                datos = (object?)null
            });
        }

        private string? GetBearerToken()
        {
            var auth = Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(auth) &&
                auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return auth.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}