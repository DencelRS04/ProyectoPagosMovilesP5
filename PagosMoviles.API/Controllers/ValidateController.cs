using Microsoft.AspNetCore.Mvc;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Services;

namespace PagosMoviles.API.Controllers
{
    [ApiController]
    [Route("validate")]
    public class ValidateController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public ValidateController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // POST /validate
        [HttpPost]
        public IActionResult ValidarToken([FromBody] ValidateTokenDto dto)
        {
            if (!ModelState.IsValid || dto == null || string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(new { codigo = -1, descripcion = "Debe enviar el token" });

            if (!_tokenService.ValidarToken(dto.Token))
                return Unauthorized(new { codigo = -1, descripcion = "Token inválido" });

            return Ok(true);
        }
    }
}