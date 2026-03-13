using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.Security;
using PagosMoviles.CoreBancarioService.Services;

namespace PagosMoviles.CoreBancarioService.Controllers
{
    [ApiController]
    [Route("core")]
    public class ClientesController : ControllerBase
    {
        private readonly CoreDbContext _db;
        private readonly CoreGatewayBitacoraClient _bitacora;

        public ClientesController(CoreDbContext db, CoreGatewayBitacoraClient bitacora)
        {
            _db = db;
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