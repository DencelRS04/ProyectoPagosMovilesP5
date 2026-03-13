using Microsoft.EntityFrameworkCore;
using PagosMoviles.UsuariosService.Models;

namespace PagosMoviles.UsuariosService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<PagoMovil> PagoMoviles { get; set; }


    }
}
