using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Data;
using PagosMoviles.API.Models;
using PagosMoviles.API.Services;
using System.Security.Cryptography;
using System.Text;

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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            var email = Request.Headers["usuario"].ToString();
            var password = Request.Headers["password"].ToString();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new
                {
                    codigo = -1,
                    descripcion = "Usuario y/o contraseña incorrectos."
                });
            }

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    codigo = -1,
                    descripcion = "Usuario y/o contraseña incorrectos."
                });
            }

            if (user.Bloqueado)
            {
                return Unauthorized(new
                {
                    codigo = -1,
                    descripcion = "Usuario bloqueado por 3 intentos fallidos."
                });
            }

            var hashedPassword = HashPassword(password);

            if (user.PasswordHash != hashedPassword)
            {
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                {
                    user.Bloqueado = true;
                }

                await _context.SaveChangesAsync();

                return Unauthorized(new
                {
                    codigo = -1,
                    descripcion = "Usuario y/o contraseña incorrectos."
                });
            }

            user.IntentosFallidos = 0;
            await _context.SaveChangesAsync();
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
                usuarioID = user.UsuarioId,
                nombreCompleto = user.NombreCompleto,
                rol = user.RolId == 1 ? "ADMIN" : "USUARIO",
                fotoPerfil = user.FotoPerfil,
                colorAvatar = string.IsNullOrWhiteSpace(user.ColorAvatar) ? "#4285F4" : user.ColorAvatar
            });
        }

        [HttpGet("validate")]
        public IActionResult Validate()
        {
            return Ok(new
            {
                codigo = 0,
                descripcion = "Token válido",
                data = true
            });
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}