using Microsoft.AspNetCore.Mvc;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Security;
using PagosMoviles.CoreBancarioService.Services;

namespace PagosMoviles.CoreBancarioService.Controllers
{
    [ApiController]
    [Route("core/accounts")]
    [ServiceFilter(typeof(CoreGatewayBearerGuardFilter))]  // ← valida token en TODOS los endpoints
    public class CoreAccountsController : ControllerBase
    {
        private readonly CoreAccountsService _service;
        private readonly CoreGatewayBitacoraClient _bitacora;

        public CoreAccountsController(CoreAccountsService service, CoreGatewayBitacoraClient bitacora)
        {
            _service = service;
            _bitacora = bitacora;
        }

        // GET /core/accounts
        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            var token = GetBearerToken();
            var resp = _service.ListarTodas();

            await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS ListarTodas OK", null, null, token);
            return Ok(new { codigo = 200, descripcion = resp.Message, datos = resp.Data });
        }

        // GET /core/accounts/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ListarPorId(int id)
        {
            var token = GetBearerToken();
            var resp = _service.ListarPorId(id);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"ACCOUNTS ListarPorId: {resp.Message}", null, new { id }, token);
                return NotFound(new { codigo = 404, descripcion = resp.Message, datos = (object?)null });
            }

            await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS ListarPorId OK", null, new { id }, token);
            return Ok(new { codigo = 200, descripcion = resp.Message, datos = resp.Data });
        }

        // GET /core/accounts/cliente/{clienteId}
        [HttpGet("cliente/{clienteId:int}")]
        public async Task<IActionResult> ListarPorCliente(int clienteId)
        {
            var token = GetBearerToken();
            var resp = _service.ListarPorCliente(clienteId);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"ACCOUNTS ListarPorCliente: {resp.Message}", null, new { clienteId }, token);
                return NotFound(new { codigo = 404, descripcion = resp.Message, datos = (object?)null });
            }

            await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS ListarPorCliente OK", null, new { clienteId }, token);
            return Ok(new { codigo = 200, descripcion = resp.Message, datos = resp.Data });
        }

        // POST /core/accounts
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearCuentaDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS Crear: datos incorrectos", null, dto, token);
                return BadRequest(new { codigo = 400, descripcion = "Datos incorrectos", datos = (object?)null });
            }

            var resp = _service.Crear(dto);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"ACCOUNTS Crear: {resp.Message}", null, dto, token);
                return BadRequest(new { codigo = 400, descripcion = resp.Message, datos = (object?)null });
            }

            await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS Crear OK", null, resp.Data, token);
            return StatusCode(201, new { codigo = 201, descripcion = resp.Message, datos = resp.Data });
        }

        // PUT /core/accounts/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarCuentaDto dto)
        {
            var token = GetBearerToken();

            if (!ModelState.IsValid)
            {
                await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS Editar: datos incorrectos", null, dto, token);
                return BadRequest(new { codigo = 400, descripcion = "Datos incorrectos", datos = (object?)null });
            }

            var resp = _service.Editar(id, dto);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"ACCOUNTS Editar: {resp.Message}", null, new { id, dto }, token);
                return NotFound(new { codigo = 404, descripcion = resp.Message, datos = (object?)null });
            }

            await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS Editar OK", new { id }, resp.Data, token);
            return Ok(new { codigo = 200, descripcion = resp.Message, datos = resp.Data });
        }

        // DELETE /core/accounts/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var token = GetBearerToken();
            var resp = _service.Eliminar(id);

            if (!resp.Success)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"ACCOUNTS Eliminar: {resp.Message}", null, new { id }, token);
                return NotFound(new { codigo = 404, descripcion = resp.Message, datos = (object?)null });
            }

            await _bitacora.RegistrarAsync("SYSTEM", "ACCOUNTS Eliminar OK", new { id }, null, token);
            return Ok(new { codigo = 200, descripcion = resp.Message, datos = (object?)null });
        }

        private string? GetBearerToken()
        {
            var auth = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(auth) &&
                auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return auth.Substring("Bearer ".Length).Trim();
            return null;
        }
    }
}