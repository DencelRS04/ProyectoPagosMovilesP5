using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Data;

namespace PagosMoviles.API.Services
{
    public class TokenService
    {
        private readonly PagosMovilesDbContext _db;

        public TokenService(PagosMovilesDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ValidarJwtAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("TOKENSERVICE: token vacío.");
                return false;
            }

            token = token.Trim();

            var registro = await _db.TokenSesiones
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.JwtToken == token);

            if (registro == null)
            {
                Console.WriteLine("TOKENSERVICE: no existe token en TokenSesiones.");
                return false;
            }

            Console.WriteLine($"TOKENSERVICE: token encontrado.");
            Console.WriteLine($"TOKENSERVICE: FechaExpiracion BD = {registro.FechaExpiracion:O}");
            Console.WriteLine($"TOKENSERVICE: UtcNow = {DateTime.UtcNow:O}");

            var valido = registro.FechaExpiracion > DateTime.UtcNow;

            Console.WriteLine($"TOKENSERVICE: vigente = {valido}");

            return valido;
        }

        public bool ValidarToken(string token)
        {
            return ValidarJwtAsync(token).GetAwaiter().GetResult();
        }
    }
}