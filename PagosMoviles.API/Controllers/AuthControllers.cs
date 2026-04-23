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
        private readonly IConfiguration _config;

        public AuthController(
            PagosMovilesDbContext context,
            JwtService jwt,
            IConfiguration config)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            var email = Request.Headers["usuario"].ToString().Trim();
            var password = Request.Headers["password"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new
                {
                    codigo = 400,
                    descripcion = "Debe ingresar usuario y contraseña."
                });
            }

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return NotFound(new
                {
                    codigo = 404,
                    descripcion = "Usuario y/o contraseña incorrectos."
                });
            }

            if (user.Bloqueado)
            {
                return StatusCode(403, new
                {
                    codigo = 403,
                    descripcion = "Usuario bloqueado por 3 intentos fallidos."
                });
            }

            var hashedPassword = HashPassword(password);

            if (user.PasswordHash != hashedPassword)
            {
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                    user.Bloqueado = true;

                await _context.SaveChangesAsync();

                return Unauthorized(new
                {
                    codigo = 401,
                    descripcion = "Usuario y/o contraseña incorrectos."
                });
            }

            user.IntentosFallidos = 0;
            await _context.SaveChangesAsync();

            int minutes = 900;
            int.TryParse(_config["Jwt:Minutes"], out minutes);
            if (minutes <= 0)
                minutes = 900;

            var jwt = _jwt.GenerarToken(user);
            var refresh = Guid.NewGuid().ToString();
            var fechaExpiracion = DateTime.UtcNow.AddMinutes(minutes);

            var tokenSesion = new TokenSesion
            {
                UsuarioId = user.UsuarioId,
                JwtToken = jwt,
                RefreshToken = refresh,
                FechaExpiracion = fechaExpiracion
            };

            _context.TokenSesiones.Add(tokenSesion);
            await _context.SaveChangesAsync();

            Console.WriteLine($"LOGIN: token generado = {jwt}");
            Console.WriteLine($"LOGIN: token guardado = {tokenSesion.JwtToken}");
            Console.WriteLine($"LOGIN: FechaExpiracion = {tokenSesion.FechaExpiracion:O}");

            return StatusCode(201, new
            {
                codigo = 201,
                descripcion = "Login exitoso",
                expires_in = fechaExpiracion,
                access_token = jwt,
                refresh_token = refresh,
                usuarioID = user.UsuarioId,
                nombreCompleto = user.NombreCompleto,
                identificacion = user.Identificacion,
                rolId = user.RolId,
                fotoPerfil = user.FotoPerfil,
                colorAvatar = string.IsNullOrWhiteSpace(user.ColorAvatar) ? "#4285F4" : user.ColorAvatar,
                clienteId = user.ClienteId

            });
        }

        [HttpGet("validate")]
        public IActionResult Validate()
        {
            return Ok(new
            {
                codigo = 200,
                descripcion = "Token válido",
                data = true
            });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}