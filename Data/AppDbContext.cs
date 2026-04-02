using BLL_ConstruccionAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace BLL_ConstruccionAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<TokenSesion> TokensSesion { get; set; }
    public DbSet<Usuario2FA> Usuarios2FA { get; set; }
    public DbSet<LogAcceso> LogAccesos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200);
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreUsuario).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.NombreUsuario).IsUnique();  // ← nuevo
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
        });

        // TokenSesion
        modelBuilder.Entity<TokenSesion>(entity =>
        {
            entity.ToTable("TokensSesion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IpOrigen).HasMaxLength(50);
        });

        // Usuario2FA
        modelBuilder.Entity<Usuario2FA>(entity =>
        {
            entity.ToTable("Usuarios2FA");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SecretKey).HasMaxLength(255).IsRequired();
        });

        // LogAcceso
        modelBuilder.Entity<LogAcceso>(entity =>
        {
            entity.ToTable("LogAccesos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IpOrigen).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(200);
        });
    }
}