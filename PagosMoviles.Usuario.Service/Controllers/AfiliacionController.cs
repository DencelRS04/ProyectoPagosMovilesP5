using Microsoft.AspNetCore.Mvc;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Services;

namespace PagosMoviles.UsuariosService.Controllers
{
    [ApiController]
    [Route("pagomovil")]
    public class AfiliacionController : ControllerBase
    {
        private readonly AfiliacionService _service;

        public AfiliacionController(AfiliacionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Inscribe un usuario a Pago Móvil
        /// </summary>
        [HttpPost("inscribir")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Inscribir([FromBody] PagoMovilDto dto)
        {
            // Validaciones de modelo (DataAnnotations)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _service.Inscribir(dto);

            if (!response.Success)
                return Conflict(response);

            // Retorna 201 Created con el objeto creado
            return Created(
                $"/pagomovil/{response.Data?.PagoMovilId}",
                response
            );
        }
    }
}
