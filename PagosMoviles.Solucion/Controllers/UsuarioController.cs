
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Services;
using PagosMoviles.UsuariosService.Models;

namespace PagosMoviles.UsuariosService.Controllers
{
    [ApiController]
    [Route("user")]
    [Produces("application/json")]
    
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;
        private readonly BitacoraClient _bitacora;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UsuarioController(
            UsuarioService service,
            BitacoraClient bitacora,
            AppDbContext context,
            IWebHostEnvironment env)
        {
            _service = service;
            _bitacora = bitacora;
            _context = context;
            _env = env;
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

        [HttpPost("actualizar-perfil")]
        public async Task<IActionResult> ActualizarPerfil([FromForm] UsuarioPerfilUpdateDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == dto.UsuarioId);

            if (usuario == null)
            {
                return NotFound(new
                {
                    codigo = -1,
                    descripcion = "Usuario no encontrado."
                });
            }

            usuario.NombreCompleto = string.IsNullOrWhiteSpace(dto.NombreCompleto)
                ? usuario.NombreCompleto
                : dto.NombreCompleto.Trim();

            usuario.Telefono = string.IsNullOrWhiteSpace(dto.Telefono)
                ? usuario.Telefono
                : dto.Telefono.Trim();

            usuario.ColorAvatar = string.IsNullOrWhiteSpace(dto.ColorAvatar)
                ? "#4285F4"
                : dto.ColorAvatar.Trim();

            if (dto.Imagen != null && dto.Imagen.Length > 0)
            {
                var webRoot = _env.WebRootPath;

                if (string.IsNullOrWhiteSpace(webRoot))
                {
                    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var carpeta = Path.Combine(webRoot, "uploads", "perfiles");

                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var extension = Path.GetExtension(dto.Imagen.FileName);
                var nombreArchivo = Guid.NewGuid().ToString() + extension;
                var rutaFisica = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await dto.Imagen.CopyToAsync(stream);
                }

                usuario.FotoPerfil = "/uploads/perfiles/" + nombreArchivo;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                codigo = 0,
                descripcion = "Perfil actualizado correctamente.",
                fotoPerfil = usuario.FotoPerfil,
                colorAvatar = usuario.ColorAvatar
            });
        }
    }
}