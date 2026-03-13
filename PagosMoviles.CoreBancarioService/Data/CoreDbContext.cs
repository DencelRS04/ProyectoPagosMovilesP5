using Microsoft.EntityFrameworkCore;
using PagosMoviles.CoreBancarioService.Models;

namespace PagosMoviles.CoreBancarioService.Data
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options)
            : base(options) { }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Cuenta> Cuentas => Set<Cuenta>();
        public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    }
}
