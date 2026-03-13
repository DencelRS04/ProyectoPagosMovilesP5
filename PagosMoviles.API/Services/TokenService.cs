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
            if (string.IsNullOrWhiteSpace(token)) return false;

            token = token.Trim();

            return await _db.TokenSesiones
                .AsNoTracking()
                .AnyAsync(t =>
                    t.JwtToken == token &&
                    t.FechaExpiracion > DateTime.UtcNow
                );
        }

        public bool ValidarToken(string token)
        {
            return ValidarJwtAsync(token).GetAwaiter().GetResult();
        }
    }
}