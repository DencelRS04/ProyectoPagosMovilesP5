using Microsoft.IdentityModel.Tokens;
using PagosMoviles.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PagosMoviles.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerarToken(Usuario usuario)
        {
            int minutes = 5;
            int.TryParse(_config["Jwt:Minutes"], out minutes);
            if (minutes <= 0) minutes = 5;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("UsuarioId", usuario.UsuarioId.ToString()),
                new Claim("RolId", usuario.RolId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}