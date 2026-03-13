using Microsoft.EntityFrameworkCore;
using PagosMoviles.UsuariosService.Models;

namespace PagosMoviles.UsuariosService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; } = default!;
    public DbSet<PagoMovil> PagoMoviles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(x => x.UsuarioId);

            entity.Property(x => x.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.TipoIdentificacion).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.Identificacion).HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.NombreCompleto).HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.Telefono).HasMaxLength(20).IsUnicode(false);
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsUnicode(false);

            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.Identificacion).IsUnique();
        });

        modelBuilder.Entity<PagoMovil>(entity =>
        {
            entity.ToTable("PagoMovil");
            entity.HasKey(x => x.PagoMovilId);

            entity.Property(x => x.Identificacion).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(x => x.NumeroCuenta).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(x => x.Telefono).HasMaxLength(20).IsUnicode(false).IsRequired();

            entity.HasIndex(x => x.Telefono).IsUnique();
            entity.HasIndex(x => x.NumeroCuenta).IsUnique();
        });
    }
}