using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Data;
using PagosMoviles.API.Models;
using PagosMoviles.API.Services;

namespace PagosMoviles.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly PagosMovilesDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(PagosMovilesDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // POST /auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            var email = Request.Headers["usuario"].ToString();
            var password = Request.Headers["password"].ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { codigo = -1, descripcion = "Todos los datos son requeridos" });

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            // ⚠️ Aquí estás comparando hash vs texto, si lo tienes hasheado debes verificar con tu PasswordHasher
            if (user == null || user.PasswordHash != password)
                return Unauthorized(new { codigo = -1, descripcion = "Usuario y/o contraseña incorrectos" });

            var jwt = _jwt.GenerarToken(user);
            var refresh = Guid.NewGuid().ToString();

            _context.TokenSesiones.Add(new TokenSesion
            {
                UsuarioId = user.UsuarioId,
                JwtToken = jwt,
                RefreshToken = refresh,
                FechaExpiracion = DateTime.Now.AddDays(1)
            });

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                codigo = 0,
                descripcion = "Login exitoso",
                expires_in = DateTime.Now.AddMinutes(5),
                access_token = jwt,
                refresh_token = refresh,
                usuarioID = user.UsuarioId
            });
        }

        // GET /auth/validate  (requiere Bearer token)
        [HttpGet("validate")]
        public IActionResult Validate()
        {
            // Si llegó aquí, significa que TokenValidationFilter lo dejó pasar
            return Ok(new { codigo = 0, descripcion = "Token válido", data = true });
        }
    }
}