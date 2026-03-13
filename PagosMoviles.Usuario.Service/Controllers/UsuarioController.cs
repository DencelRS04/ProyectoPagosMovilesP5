using Microsoft.AspNetCore.Mvc;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Services;

namespace PagosMoviles.UsuariosService.Controllers
{
    [ApiController]
    [Route("user")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult Create([FromBody] UsuarioCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _service.CrearUsuario(dto);

            if (!response.Success)
                return Conflict(response);

            return Created("", response);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _service.ActualizarUsuario(id, dto);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var response = _service.EliminarUsuario(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.ObtenerTodos());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var response = _service.ObtenerPorId(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}
