using Microsoft.EntityFrameworkCore;
using PagosMoviles.API.Models;

namespace PagosMoviles.API.Data;

public class PagosMovilesDbContext : DbContext
{
    public PagosMovilesDbContext(DbContextOptions<PagosMovilesDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; } = default!;
    public DbSet<Rol> Roles { get; set; } = default!;
    public DbSet<Pantalla> Pantallas { get; set; } = default!;
    public DbSet<RolPantalla> RolPantallas { get; set; } = default!;
    public DbSet<EntidadBancaria> EntidadesBancarias { get; set; } = default!;
    public DbSet<Parametro> Parametros { get; set; } = default!;
    public DbSet<PagoMovil> PagosMoviles { get; set; } = default!;
    public DbSet<TransaccionMovil> TransaccionesMoviles { get; set; } = default!;
    public DbSet<TokenSesion> TokenSesiones { get; set; } = default!;
    public DbSet<Bitacora> Bitacoras { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.UsuarioId);
            entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.TipoIdentificacion).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Identificacion).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.NombreCompleto).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Telefono).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.IntentosFallidos).HasDefaultValue(0);
            entity.Property(e => e.Bloqueado).HasDefaultValue(false);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne<Rol>().WithMany().HasForeignKey(e => e.RolId);
            entity.Property(e => e.FotoPerfil).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.ColorAvatar).HasMaxLength(20).IsUnicode(false);
        });

        // Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Rol");
            entity.HasKey(e => e.RolId);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
        });

        // Pantalla
        modelBuilder.Entity<Pantalla>(entity =>
        {
            entity.ToTable("Pantalla");
            entity.HasKey(e => e.PantallaId);
            entity.Property(e => e.Identificador).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Descripcion).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.Ruta).HasMaxLength(150).IsUnicode(false);
        });

        // RolPantalla
        modelBuilder.Entity<RolPantalla>(entity =>
        {
            entity.ToTable("RolPantalla");
            entity.HasKey(e => new { e.RolId, e.PantallaId });
            entity.HasOne<Rol>().WithMany().HasForeignKey(e => e.RolId);
            entity.HasOne<Pantalla>().WithMany().HasForeignKey(e => e.PantallaId);
        });

        // EntidadBancaria
        modelBuilder.Entity<EntidadBancaria>(entity =>
        {
            entity.ToTable("EntidadBancaria");
            entity.HasKey(e => e.EntidadId);
            entity.Property(e => e.CodigoEntidad).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.NombreInstitucion).HasMaxLength(150).IsUnicode(false);
            entity.HasIndex(e => e.CodigoEntidad).IsUnique();
        });

        // Parametro
        modelBuilder.Entity<Parametro>(entity =>
        {
            entity.ToTable("Parametro");
            entity.HasKey(e => e.ParametroId);
            entity.Property(e => e.ParametroId).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Valor).HasMaxLength(500).IsUnicode(false);
        });

        // PagoMovil
        modelBuilder.Entity<PagoMovil>(entity =>
        {
            entity.ToTable("PagoMovil");
            entity.HasKey(e => e.PagoMovilId);
            entity.Property(e => e.Identificacion).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.NumeroCuenta).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Telefono).HasMaxLength(20).IsUnicode(false);
            entity.HasIndex(e => e.Telefono).IsUnique();
        });

        // TransaccionMovil
        modelBuilder.Entity<TransaccionMovil>(entity =>
        {
            entity.ToTable("TransaccionMovil");
            entity.HasKey(e => e.TransaccionId);
            entity.Property(e => e.EntidadOrigen).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.EntidadDestino).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.TelefonoOrigen).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TelefonoDestino).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Descripcion).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Monto).HasPrecision(18, 2).HasColumnType("DECIMAL(18,2)");
        });

        // TokenSesion
        modelBuilder.Entity<TokenSesion>(entity =>
        {
            entity.ToTable("TokenSesion");
            entity.HasKey(e => e.TokenId);
            entity.HasOne<Usuario>().WithMany().HasForeignKey(e => e.UsuarioId);

            entity.Property(e => e.JwtToken)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();

            entity.Property(e => e.RefreshToken)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();

            entity.Property(e => e.FechaExpiracion)
                  .IsRequired();
        });

        // Bitacora
        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.ToTable("Bitacora");
            entity.HasKey(e => e.BitacoraId);
            entity.Property(e => e.Usuario).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Descripcion)
                  .HasColumnType("nvarchar(max)")
                  .IsRequired();
        });
    }
}

