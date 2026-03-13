using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Services;

namespace PagosMoviles.UsuariosService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AfiliacionController : ControllerBase
    {
        private readonly AfiliacionService _service;

        public AfiliacionController(AfiliacionService service)
        {
            _service = service;
        }

        // POST /auth/register  (PUBLICO)
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterPagoMovilDto dto)
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

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Datos incorrectos.",
                    datos = ModelState
                });
            }

            var resp = await _service.RegisterAsync(dto);

            if (resp.Codigo == 0)
            {
                return Ok(new
                {
                    codigo = 200,
                    descripcion = resp.Descripcion,
                    datos = resp.Datos
                });
            }

            return Conflict(new
            {
                codigo = 409,
                descripcion = resp.Descripcion,
                datos = resp.Datos
            });
        }

        // POST /pagomovil/inscribir
        [HttpPost("/pagomovil/inscribir")]
        public Task<IActionResult> InscribirAlias([FromBody] RegisterPagoMovilDto dto)
            => Register(dto);
    }
}